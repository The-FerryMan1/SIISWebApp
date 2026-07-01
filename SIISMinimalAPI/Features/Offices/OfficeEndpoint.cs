using System;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Offices.UpdateOffice;

namespace SIISMinimalAPI.Features.Offices;

public static class OfficeEndpoint
{
    public static IEndpointRouteBuilder MapToOffice(this IEndpointRouteBuilder app)
    {   
        var group = app.MapGroup("/office")
        .WithTags("Office")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend")
        .RequireAuthorization();


       group.MapGet("/", [Authorize(Roles = "Admin")] async Task<IResult> (IOfficeService service, CancellationToken ct) =>
        {
            var applications =  await service.GetallOfficeAsync(ct);
            return TypedResults.Ok(applications);
        }).RequireAuthorization("Admin");


        group.MapPut("/{id}", async Task<IResult> ([FromRoute] long id, UpdateOfficeDto dto, IOfficeService service, CancellationToken ct) =>
        {
            try
            {
                await service.UpdateOfficeAsync(id, dto, ct);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                
                return TypedResults.BadRequest(ex.Message);
            }
            catch(Exception ex)
            {
                return TypedResults.InternalServerError(ex.Message);
            }

        });

        return app;
    }
}
