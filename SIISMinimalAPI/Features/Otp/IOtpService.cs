namespace SIISMinimalAPI.Features.Otp;
public interface IOtpService
{
    Task SendOtpAsync(SendOtpDto dto, CancellationToken ct);
    Task<bool> VerifyOtpAsync(VerifyOtpDto dto, CancellationToken ct);
    Task<bool> ConsumeVerifiedOtpAsync(string email, Guid registrationToken, CancellationToken ct);
}