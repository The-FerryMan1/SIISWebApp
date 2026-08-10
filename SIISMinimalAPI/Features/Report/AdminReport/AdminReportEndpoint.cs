using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;

namespace SIISMinimalAPI.Features.Report.AdminReport;

public static class AdminReportEndpoint
{
    public static IEndpointRouteBuilder MapToAdminReport(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/admin")
            .WithTags("AdminReport")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/expiring", [Authorize] async Task<IResult>(long? officeId, int? days, CancellationToken ct, IAdminReportService service) =>
        {
            try
            {
                var pdf = await service.GenerateExpiringInternshipsPdf(officeId, days ?? 30, ct);
                var officePart = officeId.HasValue ? $"office{officeId.Value}_" : "all_";
                var daysPart = days ?? 30;
                return TypedResults.File(pdf, "application/pdf", $"expiring_{officePart}{daysPart}days_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
