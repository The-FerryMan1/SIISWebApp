namespace SIISMinimalAPI.Features.Otp;


public record SendOtpDto
{
    public string Email { get; set; } = string.Empty;
    public Guid RegistrationToken { get; set; }
}

public record VerifyOtpDto(string Email, Guid RegistrationToken, string Code);