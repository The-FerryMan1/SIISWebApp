using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Inbox;

public class InboxHandler(AppDbContext context) : IInboxService
{
    private readonly AppDbContext _context = context;

    public async Task<List<InboxItemDto>> GetInboxForAdminAsync(CancellationToken ct)
    {
        var items = new List<InboxItemDto>();

        var pendingApps = await _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Pending && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderByDescending(t => t.Application.CreatedAt)
            .ToListAsync(ct);

        foreach (var s in pendingApps)
        {
            items.Add(new InboxItemDto
            {
                Id = s.Application!.ApplicationUUID,
                Type = "PendingApplication",
                Title = "Pending Application",
                Description = $"New OJT application from {s.FullName} awaiting review",
                StudentName = s.FullName,
                SchoolName = s.SchoolName,
                OfficeName = s.Placement?.Office?.OfficeName,
                CreatedAt = s.Application.CreatedAt,
                ActionUrl = $"/admin/application/{s.Application.ApplicationUUID}",
                Priority = "High"
            });
        }

        return items;
    }

    public async Task<List<InboxItemDto>> GetInboxForOfficeAsync(string userId, CancellationToken ct)
    {
        var items = new List<InboxItemDto>();

        var office = await _context.Offices
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.UserId == userId && !o.IsDeleted, ct);

        if (office == null)
        {
            return items;
        }

        var students = await _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement).ThenInclude(p => p.Office)
            .Where(t => t.Placement != null && t.Placement.OfficeId == office.Id && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);

        var pendingApps = students
            .Where(t => t.Application != null && t.Application.Status == ApplicationStatusEnum.Pending)
            .OrderByDescending(t => t.Application!.CreatedAt)
            .ToList();

        foreach (var s in pendingApps)
        {
            items.Add(new InboxItemDto
            {
                Id = s.Application!.ApplicationUUID,
                Type = "PendingApplication",
                Title = "Pending Application",
                Description = $"New OJT application from {s.FullName} awaiting review",
                StudentName = s.FullName,
                SchoolName = s.SchoolName,
                OfficeName = office.OfficeName,
                CreatedAt = s.Application.CreatedAt,
                ActionUrl = $"/admin/application/{s.Application.ApplicationUUID}",
                Priority = "High"
            });
        }

        var threshold = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
        var expiring = students
            .Where(t => t.Placement != null && t.Placement.PlacementStatus == PlacementStatusEnum.Ongoing && t.Placement.EstimatedEndDate <= threshold)
            .OrderBy(t => t.Placement!.EstimatedEndDate)
            .ToList();

        foreach (var s in expiring)
        {
            items.Add(new InboxItemDto
            {
                Id = s.StudentUUID,
                Type = "ExpiringInternship",
                Title = "Internship Ending Soon",
                Description = $"{s.FullName}'s internship ends on {s.Placement!.EstimatedEndDate:MMMM dd, yyyy}",
                StudentName = s.FullName,
                SchoolName = s.SchoolName,
                OfficeName = office.OfficeName,
                CreatedAt = s.Placement!.StartDate.ToDateTime(TimeOnly.MinValue),
                ActionUrl = $"/office-dashboard",
                Priority = "Medium"
            });
        }

        var notifications = await _context.OfficeNotifications
            .Where(n => n.OfficeId == office.Id)
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

        foreach (var notification in notifications)
        {
            items.Add(new InboxItemDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Description = notification.Description,
                OfficeName = office.OfficeName,
                CreatedAt = notification.CreatedAt,
                ActionUrl = notification.ActionUrl,
                Priority = notification.Priority
            });
        }

        return items
            .OrderByDescending(i => i.CreatedAt)
            .ToList();
    }

    public async Task<int> GetInboxCountAsync(string userId, bool isAdmin, CancellationToken ct)
    {
        var items = isAdmin
            ? await GetInboxForAdminAsync(ct)
            : await GetInboxForOfficeAsync(userId, ct);

        return items.Count;
    }
}
