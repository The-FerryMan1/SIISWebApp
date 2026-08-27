using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.StudentMasterlist;

public static class StudentMasterlistEndpoint
{
    public static IEndpointRouteBuilder MapToStudentMasterlist(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/student-masterlist")
            .WithTags("StudentMasterlist")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/pdf", [Authorize] async Task<IResult>(
            [FromQuery] string? office,
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? placementStatus,
            CancellationToken ct,
            IStudentMasterlistService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Office = office,
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    PlacementStatus = placementStatus
                };

                var pdf = await service.GeneratePdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"student-masterlist.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(
            [FromQuery] string? office,
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? placementStatus,
            CancellationToken ct,
            IStudentMasterlistService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Office = office,
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    PlacementStatus = placementStatus
                };

                var csv = await service.GenerateCsv(filters, ct);
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
