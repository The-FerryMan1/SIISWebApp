using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.WeeklyReport.WeeklyReportCrud;

namespace SIISMinimalAPI.Features.WeeklyReport.WeeklyReportCrud;

public static class WeeklyReportCrudEndpoint
{
    public static IEndpointRouteBuilder MapWeeklyReportCrud(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/weekly-report")
            .WithTags("WeeklyReport")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend")
            .RequireAuthorization();

        group.MapGet("/student/{studentUuid:guid}", GetWeeklyReportsByStudentAsync);
        group.MapGet("/{weeklyReportId:int}", GetWeeklyReportAsync);
        group.MapPost("/", CreateWeeklyReportAsync);
        group.MapPut("/{weeklyReportId:int}", UpdateWeeklyReportAsync);
        group.MapDelete("/{weeklyReportId:int}", DeleteWeeklyReportAsync);

        return app;
    }

    private static async Task<IResult> GetWeeklyReportsByStudentAsync(
        Guid studentUuid,
        IWeeklyReportCrudService service,
        CancellationToken ct)
    {
        var reports = await service.GetWeeklyReportsByStudentAsync(studentUuid, ct);
        return TypedResults.Ok(reports);
    }

    private static async Task<IResult> GetWeeklyReportAsync(
        int weeklyReportId,
        IWeeklyReportCrudService service,
        CancellationToken ct)
    {
        var report = await service.GetWeeklyReportAsync(weeklyReportId, ct);
        if (report == null)
            return TypedResults.NotFound();
        return TypedResults.Ok(report);
    }

    private static async Task<IResult> CreateWeeklyReportAsync(
        CreateWeeklyReportRequest request,
        IWeeklyReportCrudService service,
        CancellationToken ct)
    {
        try
        {
            var report = await service.CreateWeeklyReportAsync(request, ct);
            return TypedResults.Created($"/api/weekly-report/{report.Id}", report);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(ex.Message);
        }
    }

    private static async Task<IResult> UpdateWeeklyReportAsync(
        int weeklyReportId,
        UpdateWeeklyReportRequest request,
        IWeeklyReportCrudService service,
        CancellationToken ct)
    {
        try
        {
            var report = await service.UpdateWeeklyReportAsync(weeklyReportId, request, ct);
            return TypedResults.Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(ex.Message);
        }
    }

    private static async Task<IResult> DeleteWeeklyReportAsync(
        int weeklyReportId,
        IWeeklyReportCrudService service,
        CancellationToken ct)
    {
        try
        {
            await service.DeleteWeeklyReportAsync(weeklyReportId, ct);
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(ex.Message);
        }
    }
}