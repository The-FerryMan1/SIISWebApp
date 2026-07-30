using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Report.Students;

namespace SIISMinimalAPI.Features.Report.Students;

public static class StudentsEndpoint
{
    public static IEndpointRouteBuilder MapToStudents(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/students")
            .WithTags("Students")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/", [Authorize] async Task<IResult>(CancellationToken ct, IStudentsService service) =>
        {
            try
            {
                var result = await service.GetStudentsPdf(ct);
                return TypedResults.File(result, "application/pdf", "students-masterlist.pdf");
            }
            catch (System.Exception ex)
            {
                return TypedResults.Problem(
                    title: "PDF Generation Failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IStudentsService service) =>
        {
            try
            {
                var result = await service.GetStudentsCsv(ct);
                return TypedResults.File(result, "text/csv", "students-masterlist.csv");
            }
            catch (System.Exception)
            {
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();

        return app;
    }
}
