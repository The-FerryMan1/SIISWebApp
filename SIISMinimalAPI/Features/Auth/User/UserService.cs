using System;
using Microsoft.AspNetCore.Identity;

namespace SIISMinimalAPI.Features.Auth.User;

public class UserService(UserManager<IdentityUser> userManager) : IUserService
{
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public async Task UserChangeInfo(string userId, UserUpdateDto userUpdateDto)
    {
        var user = await _userManager.FindByIdAsync(userId)
        ?? throw new KeyNotFoundException("User not found");
        await _userManager.SetUserNameAsync(user, userUpdateDto.Username);
    }

    public async Task<UserDto>? UserGetInfo(string userId)
    {
        var currUser = await _userManager.FindByIdAsync(userId)
        ?? throw new KeyNotFoundException("User not found");

        return new UserDto
        {
            UserId = currUser.Id,
            Email = currUser.Email,
            Username = currUser.UserName,
            IsEmailVerified = currUser.EmailConfirmed  
        };
    }
}
