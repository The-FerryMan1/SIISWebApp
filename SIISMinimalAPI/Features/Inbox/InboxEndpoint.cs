using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SIISMinimalAPI.Features.Inbox;

namespace SIISMinimalAPI.Features.Inbox;

public static class InboxEndpoint
{
    public static IEndpointRouteBuilder MapToInbox(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inbox")
            .WithTags("Inbox")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend")
            .RequireAuthorization();

        group.MapGet("/admin", [Authorize(Roles = "Admin")] 
            async Task<IResult>(CancellationToken ct, IInboxService service) =>
            {
                var result = await service.GetInboxForAdminAsync(ct);
                return TypedResults.Ok(result);
            }).RequireAuthorization("Admin");

        group.MapGet("/office", [Authorize] 
            async Task<IResult>(ClaimsPrincipal user, CancellationToken ct, IInboxService service) =>
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return TypedResults.Unauthorized();
                }
                var result = await service.GetInboxForOfficeAsync(userId, ct);
                return TypedResults.Ok(result);
            });

        group.MapGet("/count", [Authorize]
            async Task<IResult>(ClaimsPrincipal user, CancellationToken ct, IInboxService service) =>
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return TypedResults.Unauthorized();
                }

                var count = await service.GetInboxCountAsync(userId, user.IsInRole("Admin"), ct);
                return TypedResults.Ok(new { count });
            });

        return app;
    }
}
