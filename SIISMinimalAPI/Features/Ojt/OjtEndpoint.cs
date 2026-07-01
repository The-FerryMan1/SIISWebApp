using System;

namespace SIISMinimalAPI.Features.Ojt;

public static class OjtEndpoint
{
    public static IEndpointRouteBuilder MapToOjt(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/ojt")
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


        return app;
    }
}
