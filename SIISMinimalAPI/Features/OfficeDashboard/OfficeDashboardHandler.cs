using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.OfficeDashboard;

public class OfficeDashboardHandler(AppDbContext context) : IOfficeDashboardService
{
    private readonly AppDbContext _context = context;

    public async Task<OfficeDashboardDto> GetDashboardAsync(long officeId, CancellationToken ct)
    {
        var office = await _context.Offices
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == officeId && !o.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Office not found");

        var students = await _context.Students
            .Include(t => t.Application)
            .Include(t => t.Placement)
            .Where(t => t.Placement!.OfficeId == officeId && !t.IsDeleted)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);

        var approvedCount = students.Count(t => t.Application.Status == ApplicationStatusEnum.Approved);
        var pendingCount = students.Count(t => t.Application.Status == ApplicationStatusEnum.Pending);
        var rejectedCount = students.Count(t => t.Application.Status == ApplicationStatusEnum.Rejected);

        return new OfficeDashboardDto
        {
            OfficeId = office.Id,
            OfficeName = office.OfficeName,
            Department = office.Department ?? string.Empty,
            TotalStudents = students.Count,
            ApprovedCount = approvedCount,
            PendingCount = pendingCount,
            RejectedCount = rejectedCount,
            Students = students.Select(t => new StudentItemDto
            {
                StudentUuid = t.StudentUUID,
                FullName = t.FullName,
                Status = t.Application.Status.ToString(),
                School = t.SchoolName,
                StartDate = t.Placement!.StartDate,
                EstimatedEndDate = t.Placement!.EstimatedEndDate,
                TotalHours = t.TotalInternshipHours,
                AccumulatedHours = t.Placement!.AccumulatedHours,
            }).ToList(),
        };
    }
}
