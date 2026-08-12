using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIISMinimalAPI.Features.Report.PlacementUtilization;

public static class PlacementUtilizationEndpoint
{
    public static IEndpointRouteBuilder MapToPlacementUtilization(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/placement-utilization")
            .WithTags("PlacementUtilization")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/pdf", [Authorize] async Task<IResult>(CancellationToken ct, IPlacementUtilizationService service) =>
        {
            try
            {
                var pdf = await service.GeneratePdf(ct);
                return TypedResults.File(pdf, "application/pdf", "placement-utilization.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IPlacementUtilizationService service) =>
        {
            try
            {
                var csv = await service.GenerateCsv(ct);
                return TypedResults.File(csv, "application/csv", "placement-utilization.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
