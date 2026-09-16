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

        var expiryTime = now.AddMinutes(ExpiryMinutes).ToLocalTime().ToString("hh:mm tt 'GMT'zzz");
        var htmlBody = BuildOtpEmailHtml(code, expiryTime);

        await _emailService.SendEmailAsync(
            email,
            "Your verification code",
            htmlBody);
    }

    private static string BuildOtpEmailHtml(string code, string expiryTime)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
<title>Your verification code</title>
<!--[if mso]>
<noscript>
<xml>
<o:OfficeDocumentSettings>
<o:PixelsPerInch>96</o:PixelsPerInch>
</o:OfficeDocumentSettings>
</xml>
</noscript>
<![endif]-->
<style>
  body, table, td, a {{ -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }}
  table, td {{ mso-table-lspace: 0pt; mso-table-rspace: 0pt; }}
  img {{ -ms-interpolation-mode: bicubic; border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; }}
  body {{ margin: 0; padding: 0; width: 100% !important; height: 100% !important; background-color: #EEF1F6; }}

  @media screen and (max-width: 600px) {{
    .email-container {{ width: 100% !important; }}
    .fluid-padding {{ padding-left: 20px !important; padding-right: 20px !important; }}
    .otp-code {{ font-size: 34px !important; letter-spacing: 8px !important; }}
  }}

  @media (prefers-color-scheme: dark) {{
    .bg-outer {{ background-color: #0F1115 !important; }}
    .bg-card {{ background-color: #181B21 !important; }}
    .text-primary {{ color: #F1F2F4 !important; }}
    .text-secondary {{ color: #A2A7B3 !important; }}
    .otp-box {{ background-color: #1F2937 !important; border-color: #334155 !important; }}
    .divider {{ border-color: #2A2E37 !important; }}
  }}
</style>
</head>
<body class=""bg-outer"" style=""margin:0; padding:0; background-color:#EEF1F6;"">
  <!-- Preheader (hidden preview text) -->
  <div style=""display:none; max-height:0; overflow:hidden; mso-hide:all; font-size:1px; line-height:1px; color:#EEF1F6;"">
    Your one-time verification code is inside. It expires in 10 minutes.
  </div>

  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" class=""bg-outer"" style=""background-color:#EEF1F6;"">
    <tr>
      <td align=""center"" style=""padding: 40px 16px;"">

        <table role=""presentation"" width=""480"" cellpadding=""0"" cellspacing=""0"" class=""email-container"" style=""width:480px; max-width:480px;"">

          <!-- Logo / wordmark -->
          <tr>
            <td align=""center"" style=""padding-bottom: 28px;"">
              <table role=""presentation"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td style=""width:32px; height:32px; background-color:#2F6E52; border-radius:8px; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;"" align=""center"" valign=""middle"">
                    <span style=""color:#ffffff; font-size:16px; font-weight:700; line-height:32px;"">S</span>
                  </td>
                  <td style=""padding-left:10px; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif; font-size:16px; font-weight:600; color:#1A1D23;"" class=""text-primary"">
                    SIIS
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Card -->
          <tr>
            <td class=""bg-card"" style=""background-color:#FFFFFF; border-radius:16px; box-shadow: 0 1px 3px rgba(16,24,40,0.06), 0 1px 2px rgba(16,24,40,0.04);"">
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">

                <tr>
                  <td class=""fluid-padding"" style=""padding: 40px 40px 8px 40px; text-align:center;"">
                    <p class=""text-primary"" style=""margin:0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif; font-size:20px; font-weight:600; color:#1A1D23; line-height:1.3;"">
                      Verify your identity
                    </p>
                    <p class=""text-secondary"" style=""margin:10px 0 0 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif; font-size:14px; color:#667085; line-height:1.6;"">
                      Enter this code to finish signing in. It's valid for the next 10 minutes.
                    </p>
                  </td>
                </tr>

                <!-- OTP code box -->
                <tr>
                  <td class=""fluid-padding"" style=""padding: 28px 40px 8px 40px;"">
                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" class=""otp-box"" style=""background-color:#F5F7FA; border:1px solid #E4E7EC; border-radius:12px;"">
                      <tr>
                        <td align=""center"" style=""padding: 24px 16px;"">
                          <span class=""otp-code text-primary"" style=""font-family: 'IBM Plex Mono', 'SFMono-Regular', Consolas, Menlo, monospace; font-size:40px; font-weight:600; letter-spacing:14px; color:#1A1D23;"">
                            {code}
                          </span>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <tr>
                  <td align=""center"" style=""padding: 18px 40px 0 40px;"">
                    <p class=""text-secondary"" style=""margin:0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif; font-size:13px; color:#98A2B3;"">
                      Code expires at {expiryTime}
                    </p>
                  </td>
                </tr>

                <!-- Divider -->
                <tr>
                  <td style=""padding: 32px 40px 0 40px;"">
                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                      <tr><td class=""divider"" style=""border-top:1px solid #EAECF0; font-size:0; line-height:0;"">&nbsp;</td></tr>
                    </table>
                  </td>
                </tr>

                <!-- Security note -->
                <tr>
                  <td class=""fluid-padding"" style=""padding: 24px 40px 40px 40px;"">
                    <p class=""text-secondary"" style=""margin:0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif; font-size:13px; color:#98A2B3; line-height:1.6; text-align:center;"">
                      Didn't request this code? You can safely ignore this email — your account is still secure. Never share this code with anyone, including SIIS staff.
                    </p>
                  </td>
                </tr>

              </table>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td align=""center"" style=""padding: 28px 20px 0 20px;"">
              <p style=""margin:0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif; font-size:12px; color:#98A2B3; line-height:1.7;"">
                Student Internship Information System (SIIS) &middot; 123 Market Street, San Francisco, CA<br>
                <a href=""#"" style=""color:#98A2B3; text-decoration:underline;"">Help Center</a>
                &nbsp;&middot;&nbsp;
                <a href=""#"" style=""color:#98A2B3; text-decoration:underline;"">Privacy Policy</a>
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
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