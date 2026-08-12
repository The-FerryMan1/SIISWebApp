using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIISMinimalAPI.Features.Report.HoursProgress;

public static class HoursProgressEndpoint
{
    public static IEndpointRouteBuilder MapToHoursProgress(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/hours-progress")
            .WithTags("HoursProgress")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/pdf", [Authorize] async Task<IResult>(CancellationToken ct, IHoursProgressService service) =>
        {
            try
            {
                var pdf = await service.GeneratePdf(ct);
                return TypedResults.File(pdf, "application/pdf", "hours-progress.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IHoursProgressService service) =>
        {
            try
            {
                var csv = await service.GenerateCsv(ct);
                return TypedResults.File(csv, "application/csv", "hours-progress.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
