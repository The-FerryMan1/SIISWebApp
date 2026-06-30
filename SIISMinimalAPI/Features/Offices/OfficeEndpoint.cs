using System;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;

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

        return app;
    }
}
