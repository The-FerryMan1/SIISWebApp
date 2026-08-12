using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIISMinimalAPI.Features.Report.RequirementsCompliance;

public static class RequirementsComplianceEndpoint
{
    public static IEndpointRouteBuilder MapToRequirementsCompliance(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/requirements-compliance")
            .WithTags("RequirementsCompliance")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/pdf", [Authorize] async Task<IResult>(CancellationToken ct, IRequirementsComplianceService service) =>
        {
            try
            {
                var pdf = await service.GeneratePdf(ct);
                return TypedResults.File(pdf, "application/pdf", "requirements-compliance.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IRequirementsComplianceService service) =>
        {
            try
            {
                var csv = await service.GenerateCsv(ct);
                return TypedResults.File(csv, "application/csv", "requirements-compliance.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
