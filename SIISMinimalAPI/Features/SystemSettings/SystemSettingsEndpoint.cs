using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIISMinimalAPI.Features.SystemSettings;
using System.IO;

namespace SIISMinimalAPI.Features.SystemSettings;

public static class SystemSettingsEndpoint
{
    public static IEndpointRouteBuilder MapToSystemSettings(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/system-settings")
            .WithTags("SystemSettings")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend")
            .RequireAuthorization();

        group.MapGet("/", [Authorize] async Task<IResult>(ISystemSettingsService service, CancellationToken ct) =>
        {
            var settings = await service.GetSettingsAsync(ct);
            return TypedResults.Ok(settings);
        }).RequireAuthorization();

        group.MapPut("/", [Authorize(Roles = "Admin")] async Task<IResult>(
            UpdateSystemSettingsRequest request,
            ISystemSettingsService service,
            CancellationToken ct) =>
        {
            var settings = await service.UpdateSettingsAsync(request, ct);
            return TypedResults.Ok(settings);
        }).RequireAuthorization("Admin");

        group.MapPost("/logo", [Authorize(Roles = "Admin")] async Task<IResult>(
            [FromForm] IFormFile logo,
            HttpContext context,
            ISystemSettingsService service,
            CancellationToken ct) =>
        {
            if (logo is null || logo.Length == 0)
            {
                return TypedResults.BadRequest("Logo file is required.");
            }

            var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();
            if (!new[] { ".png", ".jpg", ".jpeg", ".gif", ".svg" }.Contains(extension))
            {
                return TypedResults.BadRequest("Invalid image type. Use PNG, JPG, GIF, or SVG.");
            }

            var webRootPath = Path.Combine(context.RequestServices.GetRequiredService<IWebHostEnvironment>().ContentRootPath, "wwwroot");
            var uploadsPath = Path.Combine(webRootPath, "uploads", "system");
            Directory.CreateDirectory(uploadsPath);

            var safeFileName = $"logo{extension}";
            var filePath = Path.Combine(uploadsPath, safeFileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            await using var stream = File.Create(filePath);
            await logo.CopyToAsync(stream, ct);

            var relativePath = $"/uploads/system/{safeFileName}";
            await service.UpdateSettingsAsync(new UpdateSystemSettingsRequest { LogoPath = relativePath }, ct);

            return TypedResults.Ok(new { path = relativePath });
        })
        .Accepts<IFormFile>("multipart/form-data")
        .DisableAntiforgery();

        return app;
    }
}
