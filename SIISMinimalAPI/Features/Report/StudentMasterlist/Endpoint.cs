using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIISMinimalAPI.Features.Report.StudentMasterlist;

public static class StudentMasterlistEndpoint
{
    public static IEndpointRouteBuilder MapToStudentMasterlist(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/student-masterlist")
            .WithTags("StudentMasterlist")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/pdf", [Authorize] async Task<IResult>([FromQuery] string officeName, CancellationToken ct, IStudentMasterlistService service) =>
        {
            try
            {
                var pdf = await service.GeneratePdf(officeName, ct);
                return TypedResults.File(pdf, "application/pdf", $"student-masterlist.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>([FromQuery] string officeName, CancellationToken ct, IStudentMasterlistService service) =>
        {
            try
            {
                var csv = await service.GenerateCsv(officeName, ct);
                return TypedResults.File(csv, "application/csv", $"student-masterlist.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
