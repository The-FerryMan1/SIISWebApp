using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.OfficeReport;

public static class OfficeReportEndpoint
{
    public static IEndpointRouteBuilder MapToOfficeReport(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/office")
            .WithTags("OfficeReport")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/schools", [Authorize] async Task<IResult>(AppDbContext context, CancellationToken ct) =>
        {
            var schools = await context.Students
                .Where(t => !t.IsDeleted && !string.IsNullOrEmpty(t.SchoolName))
                .Select(t => t.SchoolName!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync(ct);

            return TypedResults.Ok(schools);
        }).RequireAuthorization();

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

        group.MapGet("/masterlist", [Authorize] async Task<IResult>(
            ClaimsPrincipal user,
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? status,
            [FromQuery] string? placementStatus,
            AppDbContext context,
            CancellationToken ct,
            IOfficeReportService service) =>
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
                Status = status,
                PlacementStatus = placementStatus
            };

            try
            {
                var pdf = await service.GenerateMasterlistPdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"masterlist_{office.Id}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/ongoing", [Authorize] async Task<IResult>(
            ClaimsPrincipal user,
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? status,
            [FromQuery] string? placementStatus,
            AppDbContext context,
            CancellationToken ct,
            IOfficeReportService service) =>
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
                Status = status,
                PlacementStatus = placementStatus
            };

            try
            {
                var pdf = await service.GenerateOngoingPdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"ongoing_{office.Id}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/finished", [Authorize] async Task<IResult>(
            ClaimsPrincipal user,
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? status,
            [FromQuery] string? placementStatus,
            AppDbContext context,
            CancellationToken ct,
            IOfficeReportService service) =>
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
                Status = status,
                PlacementStatus = placementStatus
            };

            try
            {
                var pdf = await service.GenerateFinishedPdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"finished_{office.Id}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
