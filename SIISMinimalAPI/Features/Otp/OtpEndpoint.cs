using FluentValidation;

namespace SIISMinimalAPI.Features.Otp;

public static class OtpEndpoint
{
    public static IEndpointRouteBuilder MapOtp(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/otp")
            .WithTags("OTP")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend")
            .AllowAnonymous();

        group.MapPost("/send", async Task<IResult> (SendOtpDto dto, IOtpService service, CancellationToken ct) =>
        {
            try
            {
                await service.SendOtpAsync(dto, ct);
                return Results.Ok();
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ex.Errors
                    .GroupBy(t => t.PropertyName)
                    .ToDictionary(t => t.Key, t => t.Select(e => e.ErrorMessage).ToArray()));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        });

        group.MapPost("/verify", async Task<IResult> (VerifyOtpDto dto, IOtpService service, CancellationToken ct) =>
        {
            var verified = await service.VerifyOtpAsync(dto, ct);
            return verified ? Results.Ok() : Results.BadRequest("The verification code is invalid or expired.");
        });

        return app;
    }
}