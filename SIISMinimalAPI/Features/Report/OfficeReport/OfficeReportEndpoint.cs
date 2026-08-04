using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;

namespace SIISMinimalAPI.Features.Report.OfficeReport;

public static class OfficeReportEndpoint
{
    public static IEndpointRouteBuilder MapToOfficeReport(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/office")
            .WithTags("OfficeReport")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/my-office", [Authorize] async Task<IResult>(ClaimsPrincipal user, AppDbContext context, CancellationToken ct) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return TypedResults.Unauthorized();
            }

            var office = await context.Offices
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.UserId == userId && !o.IsDeleted, ct);

            if (office is null)
            {
                return TypedResults.NotFound("No office assigned to this account");
            }

            return TypedResults.Ok(new { id = office.Id, officeName = office.OfficeName });
        });

        group.MapGet("/masterlist", [Authorize] async Task<IResult>(long officeId, CancellationToken ct, IOfficeReportService service) =>
        {
            try
            {
                var pdf = await service.GenerateMasterlistPdf(officeId, ct);
                return TypedResults.File(pdf, "application/pdf", $"masterlist_{officeId}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/expiring", [Authorize] async Task<IResult>(long officeId, CancellationToken ct, IOfficeReportService service) =>
        {
            try
            {
                var pdf = await service.GenerateExpiringPdf(officeId, ct);
                return TypedResults.File(pdf, "application/pdf", $"expiring_{officeId}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/finished", [Authorize] async Task<IResult>(long officeId, CancellationToken ct, IOfficeReportService service) =>
        {
            try
            {
                var pdf = await service.GenerateFinishedPdf(officeId, ct);
                return TypedResults.File(pdf, "application/pdf", $"finished_{officeId}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
