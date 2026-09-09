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
        var officeId = await _context.Students
            .Where(s => s.Id == studentId && s.Placement != null)
            .Select(s => s.Placement!.OfficeId)
            .FirstOrDefaultAsync(ct);

        if (officeId.HasValue)
        {
            await NotifyOfficeAsync(officeId.Value, type, title, description, actionUrl, priority, ct);
        }
    }
}