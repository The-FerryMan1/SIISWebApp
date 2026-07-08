using System;

namespace SIISMinimalAPI.Features.Auth.User;

public interface IUserService
{
    Task<UserDto>? UserGetInfo(string userId);
    Task UserChangeInfo(string userId, UserUpdateDto userUpdateDto);
}
