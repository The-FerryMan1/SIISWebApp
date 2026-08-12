using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIISMinimalAPI.Features.Report.OfficePerformance;

public static class OfficePerformanceEndpoint
{
    public static IEndpointRouteBuilder MapToOfficePerformance(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/office-performance")
            .WithTags("OfficePerformance")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/pdf", [Authorize] async Task<IResult>(CancellationToken ct, IOfficePerformanceService service) =>
        {
            try
            {
                var pdf = await service.GeneratePdf(ct);
                return TypedResults.File(pdf, "application/pdf", "office-performance.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IOfficePerformanceService service) =>
        {
            try
            {
                var csv = await service.GenerateCsv(ct);
                return TypedResults.File(csv, "application/csv", "office-performance.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
