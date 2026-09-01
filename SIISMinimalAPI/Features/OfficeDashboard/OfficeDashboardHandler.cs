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

        var ongoingCount = students.Count(t => t.Placement != null && t.Placement.PlacementStatus == Shared.Enums.PlacementStatusEnum.Ongoing);
        var finishedCount = students.Count(t => t.Placement != null && t.Placement.PlacementStatus == Shared.Enums.PlacementStatusEnum.Finished);

        return new OfficeDashboardDto
        {
            OfficeId = office.Id,
            OfficeName = office.OfficeName,
            Department = office.Department ?? string.Empty,
            TotalStudents = students.Count,
            OngoingCount = ongoingCount,
            FinishedCount = finishedCount,
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
                PlacementStatus = t.Placement!.PlacementStatus.ToString()
            }).ToList(),
        };
    }
}
