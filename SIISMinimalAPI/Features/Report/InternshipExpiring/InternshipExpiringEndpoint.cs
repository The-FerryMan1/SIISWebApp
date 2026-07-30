using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Report.InternshipExpiring;

namespace SIISMinimalAPI.Features.Report.InternshipExpiring;

public static class InternshipExpiringEndpoint
{
    public static IEndpointRouteBuilder MapToInternshipExpiring(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/internship/expiring")
            .WithTags("InternshipExpiring")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/", [Authorize] async Task<IResult>(CancellationToken ct, IInternshipExpiringService service) =>
        {
            try
            {
                var result = await service.GetExpiringInternships(ct);
                return TypedResults.File(result, "application/pdf", "expiring-internships.pdf");
            }
            catch (System.Exception ex)
            {
                return TypedResults.Problem(
                    title: "PDF Generation Failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IInternshipExpiringService service) =>
        {
            try
            {
                var result = await service.GetExpiringInternshipsCsv(ct);
                return TypedResults.File(result, "text/csv", "expiring-internships.csv");
            }
            catch (System.Exception)
            {
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();

        group.MapGet("/days", [Authorize] async Task<IResult>([FromQuery] int days, CancellationToken ct, IInternshipExpiringService service) =>
        {
            try
            {
                if (days <= 0 || days > 365)
                {
                    return TypedResults.BadRequest("Days must be between 1 and 365");
                }
                var result = await service.GetExpiringInternships(ct, days);
                return TypedResults.File(result, "application/pdf", $"expiring-internships-{days}days.pdf");
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
