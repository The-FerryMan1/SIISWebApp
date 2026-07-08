using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SIISMinimalAPI.Features.Auth.User;

public static class UserEndpoint
{
    public static IEndpointRouteBuilder MapToUser(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/user")
        .WithTags("User")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend")
        .RequireAuthorization(["Admin"]);


        group.MapGet("/", [Authorize] async Task<IResult> (ClaimsPrincipal user, IUserService service) =>
        {
            try
            {
                var currUser = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var authUser = await service.UserGetInfo(currUser);
                return TypedResults.Ok(authUser);
            }
            catch (KeyNotFoundException ex)
            {

                return TypedResults.Unauthorized();
            }
        });

        group.MapPut("/", [Authorize] async Task<IResult> (ClaimsPrincipal user, UserUpdateDto userUpdateDto, IUserService service) =>
        {
            try
            {
                var currUser = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await service.UserChangeInfo(currUser, userUpdateDto);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.Unauthorized();
            }
        });

        return app;
    }
}
