using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;

namespace SIISMinimalAPI.Features.SystemSettings;

public class SystemSettingsHandler(AppDbContext context) : ISystemSettingsService
{
    private readonly AppDbContext _context = context;

    public async Task<SystemSettingsDto> GetSettingsAsync(CancellationToken ct)
    {
        try
        {
            var settings = await _context.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == 1, ct);

            if (settings is null)
            {
                settings = new SystemSettings
                {
                    Id = 1,
                    LogoPath = null,
                    ThemeColor = "#2f5cba",
                    UpdatedAt = DateTime.Now
                };
                await _context.SystemSettings.AddAsync(settings, ct);
                await _context.SaveChangesAsync(ct);
            }

            return new SystemSettingsDto
            {
                LogoPath = settings.LogoPath,
                ThemeColor = settings.ThemeColor
            };
        }
        catch (Exception)
        {
            await EnsureTableExistsAsync(ct);
            return new SystemSettingsDto
            {
                LogoPath = null,
                ThemeColor = "#2f5cba"
            };
        }
    }

    public async Task<SystemSettingsDto> UpdateSettingsAsync(UpdateSystemSettingsRequest request, CancellationToken ct)
    {
        try
        {
            var settings = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Id == 1, ct);

            if (settings is null)
            {
                settings = new SystemSettings { Id = 1 };
                await _context.SystemSettings.AddAsync(settings, ct);
            }

            if (request.LogoPath is not null)
            {
                settings.LogoPath = request.LogoPath;
            }

            if (request.ThemeColor is not null)
            {
                settings.ThemeColor = request.ThemeColor;
            }

            settings.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync(ct);

            return new SystemSettingsDto
            {
                LogoPath = settings.LogoPath,
                ThemeColor = settings.ThemeColor
            };
        }
        catch (Exception)
        {
            await EnsureTableExistsAsync(ct);
            throw;
        }
    }

    private async Task EnsureTableExistsAsync(CancellationToken ct)
    {
        try
        {
            var tableExists = await _context.Database
                .ExecuteSqlInterpolatedAsync($"""
                    CREATE TABLE IF NOT EXISTS "SystemSettings" (
                        "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        "LogoPath" TEXT NULL,
                        "ThemeColor" TEXT NULL,
                        "UpdatedAt" TEXT NULL
                    )
                    """, ct);

            await _context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT OR IGNORE INTO "SystemSettings" ("Id", "LogoPath", "ThemeColor", "UpdatedAt")
                VALUES (1, NULL, '#2f5cba', '{DateTime.Now:O}')
                """, ct);
        }
        catch
        {
            // ignore table creation errors
        }
    }
}
