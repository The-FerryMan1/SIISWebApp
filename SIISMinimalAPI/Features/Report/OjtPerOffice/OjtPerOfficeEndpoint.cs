using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.OjtPerOffice;

public static class OjtPerOfficeEndpoint
{
    public static IEndpointRouteBuilder MapToOjtPerOffice(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/report/ojtPerOffice")
        .WithTags("OjtPerOffice")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend");



        group.MapGet("/", [Authorize] async Task<IResult> (string office, CancellationToken ct, IOjtPerOfficeService service) =>
           {
               try
               {
                   var pdf = await service.ListAllOjtPerOffice(office, ct);
                   return TypedResults.File(pdf, "application/pdf", $"OJT_{office}_{DateTime.Now:yyyyMMdd}.pdf");
               }
               catch (System.Exception ex)
               {

                   return TypedResults.Problem(
           title: "PDF Generation Failed",
           detail: ex.Message,
           statusCode: StatusCodes.Status500InternalServerError
       );
               }
           }).RequireAuthorization();

        group.MapGet("/filtered", [Authorize] async Task<IResult>(
            [FromQuery] string? office,
            [FromQuery] ApplicationStatusEnum? status,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            CancellationToken ct,
            IOjtPerOfficeService service) =>
        {
            try
            {
                var pdf = await service.ListAllOjtPerOfficeFiltered(office, status, dateFrom, dateTo, ct);
                return TypedResults.File(pdf, "application/pdf", $"OJT_filtered_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (System.Exception ex)
            {
                return TypedResults.Problem(
                    title: "PDF Generation Failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }).RequireAuthorization();

        

        return app;
    }
}
