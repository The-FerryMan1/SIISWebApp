using System;
using System.Reflection.Metadata.Ecma335;

namespace SIISMinimalAPI.Features.Application;

public static class  ApplicationEndpoint
{
    public static IEndpointRouteBuilder MapToApplication(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/application")
        .WithTags("Application")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend");

        group.MapGet("/", async Task<IResult> (IApplicationService service, CancellationToken ct) =>
        {
            var applications =  await service.GetAllAsync(ct);
           return TypedResults.Ok(applications);
        });

        group.MapGet("/{uuid}", async Task<IResult> (Guid uuid, IApplicationService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.GetByIdAsync(uuid, ct);
                return TypedResults.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                
                return TypedResults.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return TypedResults.InternalServerError(ex.Message);
            }
        });

        return app;
    }
}
