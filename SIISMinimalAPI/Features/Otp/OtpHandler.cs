using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Email;
using SIISMinimalAPI.Features.RegistrationToken;
using OtpModel = SIISMinimalAPI.Features.Shared.Models.Otp;

namespace SIISMinimalAPI.Features.Otp;

public sealed class OtpHandler(
    AppDbContext context,
    IEmailService emailService,
    IRegistrationTokenService registrationTokenService) : IOtpService
{
    private const int ExpiryMinutes = 10;
    private readonly AppDbContext _context = context;
    private readonly IEmailService _emailService = emailService;
    private readonly IRegistrationTokenService _registrationTokenService = registrationTokenService;

    public async Task SendOtpAsync(SendOtpDto dto, CancellationToken ct)
    {
        await EnsureTableExistsAsync(ct);

        var validator = new Validators.SendOtpValidator();
        await validator.ValidateAndThrowAsync(dto, ct);

        if (!await _registrationTokenService.VerifyRegistrationToken(dto.RegistrationToken, ct))
            throw new UnauthorizedAccessException("The registration link is invalid or expired.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var identifier = BuildIdentifier(email, dto.RegistrationToken);
        var now = DateTime.UtcNow;
        var activeOtps = await _context.Otps
            .Where(t => t.Identifier == identifier && !t.IsVerified && t.Exp > now)
            .ToListAsync(ct);
        foreach (var activeOtp in activeOtps)
            activeOtp.Exp = now;

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        _context.Otps.Add(new OtpModel
        {
            Identifier = identifier,
            HashToken = Hash(code),
            CreatedAt = now,
            Exp = now.AddMinutes(ExpiryMinutes),
            IsVerified = false
        });
        await _context.SaveChangesAsync(ct);

        await _emailService.SendEmailAsync(
            email,
            "Your SIIS verification code",
            $"<p>Your SIIS verification code is <strong>{code}</strong>.</p><p>This code expires in {ExpiryMinutes} minutes.</p>");
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpDto dto, CancellationToken ct)
    {
        await EnsureTableExistsAsync(ct);
        var identifier = BuildIdentifier(dto.Email.Trim().ToLowerInvariant(), dto.RegistrationToken);
        var otp = await _context.Otps
            .Where(t => t.Identifier == identifier && !t.IsVerified && t.Exp > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otp is null || !CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(otp.HashToken),
                Convert.FromHexString(Hash(dto.Code.Trim()))))
            return false;

        otp.IsVerified = true;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ConsumeVerifiedOtpAsync(string email, Guid registrationToken, CancellationToken ct)
    {
        await EnsureTableExistsAsync(ct);
        var identifier = BuildIdentifier(email.Trim().ToLowerInvariant(), registrationToken);
        var otp = await _context.Otps
            .Where(t => t.Identifier == identifier && t.IsVerified && t.Exp > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otp is null)
            return false;

        _context.Otps.Remove(otp);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task EnsureTableExistsAsync(CancellationToken ct)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS "Otps" (
                    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    "Identifier" TEXT NOT NULL,
                    "HashToken" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "Exp" TEXT NOT NULL,
                    "IsVerified" INTEGER NOT NULL DEFAULT 0
                )
                """, ct);
        }
        catch
        {
            // ignore table creation errors
        }
    }

    private static string BuildIdentifier(string email, Guid registrationToken) => $"{registrationToken:N}:{email}";

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}