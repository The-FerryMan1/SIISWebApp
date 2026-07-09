using System;

namespace SIISMinimalAPI.Features.Ojt;

public static class OjtEndpoint
{
    public static IEndpointRouteBuilder MapToOjt(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/api/ojt")
        .WithTags("Ojt")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend")
        .RequireAuthorization();


        group.MapGet("/", async Task<IResult> (IOjtService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.GetAllOjtAsync(ct);
                return TypedResults.Ok(result);
            }
            catch (System.Exception)
            {
                
                return TypedResults.InternalServerError();
            }
        });

        group.MapGet("/{uuid}", async Task<IResult> (Guid uuid, CancellationToken ct, IOjtService service) =>
        {
            try
            {
                var result = await service.GetOjtById(uuid, ct);
                return TypedResults.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                
                return TypedResults.NotFound(ex.Message);
            }
        });

        group.MapDelete("/{uuid}", async Task<IResult> (Guid uuid, CancellationToken ct, IOjtService service) =>
        {
            try
            {
                await service.DeleteOjt(uuid, ct);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                
                return TypedResults.NotFound(ex.Message);
            }
        });


        return app;
    }
}
