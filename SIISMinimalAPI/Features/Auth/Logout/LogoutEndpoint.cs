using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace SIISMinimalAPI.Features.Auth.Logout;

public static class LogoutEndpoint
{
    public static IEndpointRouteBuilder MapToAuth(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
        .WithTags("Auth")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend");


        app.MapPost("/logout", [Authorize] async (HttpContext context) =>
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Results.Ok();
        });

        return app;
    }
}