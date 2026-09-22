using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Notifications;

public class NotificationService(AppDbContext context) : INotificationService
{
    private readonly AppDbContext _context = context;

    public async Task NotifyOfficeAsync(
        long officeId,
        string type,
        string title,
        string description,
        string actionUrl,
        string priority,
        CancellationToken ct = default)
    {
        await EnsureTableExistsAsync(ct);

        var officeExists = await _context.Offices
            .AnyAsync(o => o.Id == officeId && !o.IsDeleted, ct);

        if (!officeExists)
        {
            return;
        }

        _context.Set<OfficeNotification>().Add(new OfficeNotification
        {
            OfficeId = officeId,
            Type = type,
            Title = title,
            Description = description,
            ActionUrl = actionUrl,
            Priority = priority,
            CreatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync(ct);
    }

    public async Task NotifyOfficeForStudentAsync(
        long studentId,
        string type,
        string title,
        string description,
        string actionUrl,
        string priority,
        CancellationToken ct = default)
    {
        await EnsureTableExistsAsync(ct);

        var officeId = await _context.Students
            .Where(s => s.Id == studentId && s.Placement != null)
            .Select(s => s.Placement!.OfficeId)
            .FirstOrDefaultAsync(ct);

        if (officeId.HasValue)
        {
            await NotifyOfficeAsync(officeId.Value, type, title, description, actionUrl, priority, ct);
        }
    }

    private async Task EnsureTableExistsAsync(CancellationToken ct)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS "OfficeNotifications" (
                    "Id" TEXT NOT NULL PRIMARY KEY,
                    "OfficeId" INTEGER NOT NULL,
                    "Type" TEXT NOT NULL,
                    "Title" TEXT NOT NULL,
                    "Description" TEXT NOT NULL,
                    "ActionUrl" TEXT NOT NULL,
                    "Priority" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "ReadAt" TEXT NULL
                )
                """, ct);
        }
        catch
        {
            // ignore table creation errors
        }
    }
}