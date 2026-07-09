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
        .RequireCors("AllowFrontend")
        .RequireAuthorization("Admin");


        app.MapPost("/logout", [Authorize(Roles = "Admin")] async (HttpContext context) =>
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Results.Ok();
        }).RequireAuthorization("Admin");

        return app;
    }
}