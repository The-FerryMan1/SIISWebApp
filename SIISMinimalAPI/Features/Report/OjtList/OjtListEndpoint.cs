using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.OjtList;

public static class OjtListEndpoint
{
    public static IEndpointRouteBuilder MapToOjtList(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/ojtList")
        .WithTags("ojtList")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend");


        group.MapGet("/", [Authorize] async Task<IResult>(ApplicationStatusEnum status, CancellationToken ct, IOjtListService service) =>
        {
            try
            {
                var result = await service.ListAllOjt(status, ct);
                return TypedResults.File(result, "application/pdf", "ojt-list");
            }
            catch (System.Exception)
            {
                
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();



        group.MapGet("/csv", [Authorize] async Task<IResult>(CancellationToken ct, IOjtListService service) =>
        {
            try
            {
                var result = await service.OjtListCsv( ct);
                return TypedResults.File(result, "application/csv", "ojt-list.csv");
            }
            catch (System.Exception)
            {
                
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();

        group.MapGet("/filtered", [Authorize] async Task<IResult>(
            [FromQuery] ApplicationStatusEnum? status,
            [FromQuery] string? office,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            CancellationToken ct,
            IOjtListService service) =>
        {
            try
            {
                var result = await service.ListAllOjtFiltered(status, office, dateFrom, dateTo, ct);
                return TypedResults.File(result, "application/pdf", "ojt-list-filtered");
            }
            catch (System.Exception)
            {
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();

        group.MapGet("/csv/filtered", [Authorize] async Task<IResult>(
            [FromQuery] string? office,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            CancellationToken ct,
            IOjtListService service) =>
        {
            try
            {
                var result = await service.OjtListCsvFiltered(office, dateFrom, dateTo, ct);
                return TypedResults.File(result, "application/csv", "ojt-list-filtered.csv");
            }
            catch (System.Exception)
            {
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();

        return app;
    }
}
