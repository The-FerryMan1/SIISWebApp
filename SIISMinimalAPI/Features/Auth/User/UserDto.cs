using System;

namespace SIISMinimalAPI.Features.Auth.User;

public class UserDto
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public bool IsEmailVerified {get; set;}
}

public class UserUpdateDto
{
    public string? Email { get; set; } = string.Empty;
    public string? Username {get; set;} = string.Empty;
}
