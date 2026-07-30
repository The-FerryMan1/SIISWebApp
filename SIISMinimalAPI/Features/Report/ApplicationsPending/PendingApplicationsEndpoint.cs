using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Report.ApplicationsPending;

namespace SIISMinimalAPI.Features.Report.ApplicationsPending;

public static class PendingApplicationsEndpoint
{
    public static IEndpointRouteBuilder MapToPendingApplications(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/applications/pending")
            .WithTags("ApplicationsPending")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/", [Authorize] async Task<IResult>(CancellationToken ct, IPendingApplicationsService service) =>
        {
            try
            {
                var result = await service.GetPendingApplications(ct);
                return TypedResults.File(result, "application/pdf", "pending-applications.pdf");
            }
            catch (System.Exception ex)
            {
                return TypedResults.Problem(
                    title: "PDF Generation Failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IPendingApplicationsService service) =>
        {
            try
            {
                var result = await service.GetPendingApplicationsCsv(ct);
                return TypedResults.File(result, "text/csv", "pending-applications.csv");
            }
            catch (System.Exception)
            {
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();

        return app;
    }
}
