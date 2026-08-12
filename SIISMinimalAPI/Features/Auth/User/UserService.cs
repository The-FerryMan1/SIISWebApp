using System;
using Microsoft.AspNetCore.Identity;
using SIISMinimalAPI.Features.Shared.Models;
using SIISMinimalAPI.Features.Logs;

namespace SIISMinimalAPI.Features.Auth.User;

public class UserService(UserManager<SIISMinimalAPI.Features.Shared.Models.User> userManager, ILogService logService) : IUserService
{
    private readonly UserManager<SIISMinimalAPI.Features.Shared.Models.User> _userManager = userManager;
    private readonly ILogService _logService = logService;

    public async Task UserChangeInfo(string userId, UserUpdateDto userUpdateDto)
    {
        var user = await _userManager.FindByIdAsync(userId)
        ?? throw new KeyNotFoundException("User not found");
        
        if (!string.IsNullOrEmpty(userUpdateDto.Username))
        {
            await _userManager.SetUserNameAsync(user, userUpdateDto.Username);
        }
        
        if (!string.IsNullOrEmpty(userUpdateDto.Email))
        {
            await _userManager.SetEmailAsync(user, userUpdateDto.Email);
        }
        
        if (!string.IsNullOrEmpty(userUpdateDto.LastName))
        {
            user.LastName = userUpdateDto.LastName;
        }
        
        if (!string.IsNullOrEmpty(userUpdateDto.FirstName))
        {
            user.FirstName = userUpdateDto.FirstName;
        }
        
        if (!string.IsNullOrEmpty(userUpdateDto.MiddleName))
        {
            user.MiddleName = userUpdateDto.MiddleName;
        }
        
        await _userManager.UpdateAsync(user);

        await _logService.WriteAsync("Update", "User", null, userId, $"Updated user {user.UserName}");
    }

    public async Task UserChangePassword(string userId, UserChangePass userChangePass)
    {
         var user = await _userManager.FindByIdAsync(userId)
        ?? throw new KeyNotFoundException("User not found");

        var result = await _userManager.ChangePasswordAsync(user, userChangePass.CurrentPassword, userChangePass.NewPassword);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Wrong Current Password");
        }
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
            LastName = currUser.LastName,
            FirstName = currUser.FirstName,
            MiddleName = currUser.MiddleName,
            IsEmailVerified = currUser.EmailConfirmed  
        };
    }
}
