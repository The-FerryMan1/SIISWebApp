using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Report.RequirementsMissing;

namespace SIISMinimalAPI.Features.Report.RequirementsMissing;

public static class MissingRequirementsEndpoint
{
    public static IEndpointRouteBuilder MapToMissingRequirements(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/requirements/missing")
            .WithTags("RequirementsMissing")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/", [Authorize] async Task<IResult>(CancellationToken ct, IMissingRequirementsService service) =>
        {
            try
            {
                var result = await service.GetMissingRequirements(ct);
                return TypedResults.File(result, "application/pdf", "missing-requirements.pdf");
            }
            catch (System.Exception ex)
            {
                return TypedResults.Problem(
                    title: "PDF Generation Failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IMissingRequirementsService service) =>
        {
            try
            {
                var result = await service.GetMissingRequirementsCsv(ct);
                return TypedResults.File(result, "text/csv", "missing-requirements.csv");
            }
            catch (System.Exception)
            {
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();

        return app;
    }
}
