using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIISMinimalAPI.Features.Report.RejectedApplications;

public static class RejectedApplicationsEndpoint
{
    public static IEndpointRouteBuilder MapToRejectedApplications(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/rejected-applications")
            .WithTags("RejectedApplications")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/pdf", [Authorize] async Task<IResult>(CancellationToken ct, IRejectedApplicationsService service) =>
        {
            try
            {
                var pdf = await service.GeneratePdf(ct);
                return TypedResults.File(pdf, "application/pdf", "rejected-applications.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IRejectedApplicationsService service) =>
        {
            try
            {
                var csv = await service.GenerateCsv(ct);
                return TypedResults.File(csv, "application/csv", "rejected-applications.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
