using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Report.WeeklyReport;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.WeeklyReport;

public static class WeeklyReportEndpoint
{
    public static IEndpointRouteBuilder MapToWeeklyReport(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/weekly")
            .WithTags("WeeklyReport")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend")
            .RequireAuthorization();

        group.MapGet("/pdf", [Authorize] async Task<IResult>(
            ClaimsPrincipal user,
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? placementStatus,
            AppDbContext context,
            CancellationToken ct,
            IWeeklyReportService service) =>
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

            var filters = new CommonFilterOptions
            {
                Office = office.OfficeName,
                Name = name,
                School = school,
                DateFrom = dateFrom,
                DateTo = dateTo,
                PlacementStatus = placementStatus
            };

            try
            {
                var pdf = await service.GenerateWeeklyPdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"weekly_report_{office.Id}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/csv", [Authorize] async Task<IResult>(
            ClaimsPrincipal user,
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? placementStatus,
            AppDbContext context,
            CancellationToken ct,
            IWeeklyReportService service) =>
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

            var filters = new CommonFilterOptions
            {
                Office = office.OfficeName,
                Name = name,
                School = school,
                DateFrom = dateFrom,
                DateTo = dateTo,
                PlacementStatus = placementStatus
            };

            try
            {
                var csv = await service.GenerateWeeklyCsv(filters, ct);
                return TypedResults.File(csv, "text/csv", $"weekly_report_{office.Id}_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/history", [Authorize] async Task<IResult>(
            ClaimsPrincipal user,
            AppDbContext context,
            CancellationToken ct,
            IWeeklyReportService service) =>
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

            var history = await service.GetWeeklyHistory(office.OfficeName, ct);
            return TypedResults.Ok(history);
        }).RequireAuthorization();

        return app;
    }
}
