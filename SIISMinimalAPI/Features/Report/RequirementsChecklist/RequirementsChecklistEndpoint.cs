using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Report.RequirementsChecklist;

namespace SIISMinimalAPI.Features.Report.RequirementsChecklist;

public static class RequirementsChecklistEndpoint
{
    public static IEndpointRouteBuilder MapToRequirementsChecklist(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/requirements/checklist")
            .WithTags("RequirementsChecklist")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/", [Authorize] async Task<IResult>(CancellationToken ct, IRequirementsChecklistService service) =>
        {
            try
            {
                var result = await service.GetRequirementsChecklist(ct);
                return TypedResults.File(result, "application/pdf", "requirements-checklist.pdf");
            }
            catch (System.Exception ex)
            {
                return TypedResults.Problem(
                    title: "PDF Generation Failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IRequirementsChecklistService service) =>
        {
            try
            {
                var result = await service.GetRequirementsChecklistCsv(ct);
                return TypedResults.File(result, "text/csv", "requirements-checklist.csv");
            }
            catch (System.Exception)
            {
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();

        return app;
    }
}
