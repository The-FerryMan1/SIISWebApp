using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.AdminReport;

public static class AdminReportEndpoint
{
    public static IEndpointRouteBuilder MapToAdminReport(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/admin")
            .WithTags("AdminReport")
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

        group.MapGet("/expiring", [Authorize] async Task<IResult>(
            long? officeId,
            int? days,
            string? school,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var pdf = await service.GenerateExpiringInternshipsPdf(officeId, days ?? 30, school, dateFrom, dateTo, ct);
                var officePart = officeId.HasValue ? $"office{officeId.Value}_" : "all_";
                var daysPart = days ?? 30;
                return TypedResults.File(pdf, "application/pdf", $"expiring_{officePart}{daysPart}days_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/masterlist/pdf", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? office,
            [FromQuery] string? placementStatus,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Office = office,
                    PlacementStatus = placementStatus
                };

                var pdf = await service.GenerateMasterlistPdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"masterlist_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/masterlist/csv", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? office,
            [FromQuery] string? placementStatus,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Office = office,
                    PlacementStatus = placementStatus
                };

                var csv = await service.GenerateMasterlistCsv(filters, ct);
                return TypedResults.File(csv, "application/csv", $"masterlist_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/ongoing/pdf", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? office,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Office = office
                };

                var pdf = await service.GenerateOngoingPdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"ongoing_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/ongoing/csv", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? office,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Office = office
                };

                var csv = await service.GenerateOngoingCsv(filters, ct);
                return TypedResults.File(csv, "application/csv", $"ongoing_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/finished/pdf", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? office,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Office = office
                };

                var pdf = await service.GenerateFinishedPdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"finished_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/finished/csv", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? office,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Office = office
                };

                var csv = await service.GenerateFinishedCsv(filters, ct);
                return TypedResults.File(csv, "application/csv", $"finished_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/rejected/pdf", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                var pdf = await service.GenerateRejectedPdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"rejected_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/rejected/csv", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                var csv = await service.GenerateRejectedCsv(filters, ct);
                return TypedResults.File(csv, "application/csv", $"rejected_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/approved/pdf", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? office,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Office = office
                };

                var pdf = await service.GenerateApprovedPdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"approved_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/approved/csv", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? office,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Office = office
                };

                var csv = await service.GenerateApprovedCsv(filters, ct);
                return TypedResults.File(csv, "application/csv", $"approved_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/pending/pdf", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                var pdf = await service.GeneratePendingPdf(filters, ct);
                return TypedResults.File(pdf, "application/pdf", $"pending_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "PDF Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        group.MapGet("/pending/csv", [Authorize] async Task<IResult>(
            [FromQuery] string? name,
            [FromQuery] string? school,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            CancellationToken ct,
            IAdminReportService service) =>
        {
            try
            {
                var filters = new CommonFilterOptions
                {
                    Name = name,
                    School = school,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                var csv = await service.GeneratePendingCsv(filters, ct);
                return TypedResults.File(csv, "application/csv", $"pending_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(title: "CSV Generation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization();

        return app;
    }
}
