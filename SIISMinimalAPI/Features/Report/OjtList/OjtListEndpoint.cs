using System;
using Microsoft.AspNetCore.Authorization;
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

        return app;
    }
}
