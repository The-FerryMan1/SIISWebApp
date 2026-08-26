using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.CompletionSummary;

public static class CompletionSummaryEndpoint
{
    public static IEndpointRouteBuilder MapToCompletionSummary(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/completion-summary")
            .WithTags("CompletionSummary")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/pdf", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            CancellationToken ct,
            ICompletionSummaryService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                var pdf = await service.GeneratePdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", "completion-summary.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            CancellationToken ct,
            ICompletionSummaryService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                var csv = await service.GenerateCsv(filters, ct);
                return TypedResults.File(csv, "application/csv", "completion-summary.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
