using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.Progress;

namespace SIISMinimalAPI.Features.Progress;

public static class ProgressEndpoint
{
    public static IEndpointRouteBuilder MapToProgress(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/progress")
            .WithTags("Progress")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend");

        group.MapGet("/{studentUuid:guid}", [Authorize] async Task<IResult>(
            Guid studentUuid,
            CancellationToken ct,
            IProgressService service) =>
        {
            try
            {
                var result = await service.GetProgressByStudentUuid(studentUuid, ct);
                return TypedResults.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
            catch (System.Exception)
            {
                return TypedResults.InternalServerError();
            }
        }).RequireAuthorization();

        return app;
    }
}
