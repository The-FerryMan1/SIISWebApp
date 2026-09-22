namespace SIISMinimalAPI.Features.Daily.CreateDaily;


public static class Endpoint
{
    public static IEndpointRouteBuilder CreateDailyRoute(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/daily")
            .WithTags("Daily")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");


        group.MapPost("/", async (Request request, IService service, CancellationToken ct) =>
        {
            Validator rules = new();
            var result = await rules.ValidateAsync(request, ct);

            if (!result.IsValid)
            {
                return Results.BadRequest(result.Errors);
            }


            var daily = await service.CreateDailyAsync(request, ct);

            return Results.Created();
        });

        return app;
    }
}