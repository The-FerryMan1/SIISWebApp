using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SIISMinimalAPI.Features.Auth.User;

public static class UserEndpoint
{
    public static IEndpointRouteBuilder MapToUser(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/api/user")
        .WithTags("User")
        .RequireRateLimiting("standard")
        .RequireCors("AllowFrontend")
        .RequireAuthorization();


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

        group.MapPut("/change-password", [Authorize] async Task<IResult> (ClaimsPrincipal user, UserChangePass userChangePass, IUserService service) =>
        {
            try
            {
                var currUser = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await service.UserChangePassword(currUser, userChangePass);
                return TypedResults.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return TypedResults.Unauthorized();
            }
             catch (InvalidOperationException ex)
            {
                return TypedResults.Unauthorized();
            }
        });

        return app;
    }
}
