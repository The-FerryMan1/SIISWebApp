using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIISMinimalAPI.Features.Report.ImportAudit;

public static class ImportAuditEndpoint
{
    public static IEndpointRouteBuilder MapToImportAudit(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/import-audit")
            .WithTags("ImportAudit")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IImportAuditService service) =>
        {
            try
            {
                var csv = await service.GenerateCsv(ct);
                return TypedResults.File(csv, "application/csv", "import-audit.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
