using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Report.OfficesSummary;

namespace SIISMinimalAPI.Features.Report.OfficesSummary;

public static class OfficesSummaryEndpoint
{
    public static IEndpointRouteBuilder MapToOfficesSummary(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/offices/summary")
            .WithTags("OfficesSummary")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/", [Authorize] async Task<IResult>(CancellationToken ct, IOfficesSummaryService service) =>
        {
            try
            {
                var result = await service.GetOfficesSummary(ct);
                return TypedResults.File(result, "application/pdf", "offices-summary.pdf");
            }
            catch (System.Exception ex)
            {
                return TypedResults.Problem(
                    title: "PDF Generation Failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
