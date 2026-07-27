using System;
using Microsoft.AspNetCore.Authorization;
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



        group.MapGet("/", [Authorize] async Task<IResult> (OfficeNameEnum office, CancellationToken ct, IOjtPerOfficeService service) =>
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
            detail: ex.Message, // Only the message string, not the whole exception
            statusCode: StatusCodes.Status500InternalServerError
        );
               }
           }).RequireAuthorization();

           

        return app;
    }
}
