using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Progress.GetProgress;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Progress;

public class ProgressHandler(AppDbContext context) : IProgressService
{
    private readonly AppDbContext _context = context;

    public async Task<ProgressDto> GetProgressByStudentUuid(Guid studentUuid, CancellationToken ct)
    {
        var student = await _context.Students
            .Include(t => t.Placement)
            .ThenInclude(p => p.Office)
            .FirstOrDefaultAsync(t => t.StudentUUID == studentUuid && !t.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Student not found");

        var placement = student.Placement;
        if (placement == null)
        {
            throw new KeyNotFoundException("Placement not found for this student");
        }

        var latestProgress = placement.Progresses?
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();

        var totalHours = student.TotalInternshipHours;
        var accumulatedHours = placement.AccumulatedHours;
        var remainingHours = latestProgress?.RemainingHours ?? Math.Max(0, totalHours - accumulatedHours);
        var trainingHoursRendered = latestProgress?.TrainingHoursRendered ?? accumulatedHours;
        var trainingHoursForWeek = latestProgress?.TrainingHoursForWeek ?? 0;
        var progressPercent = totalHours > 0 ? Math.Round((accumulatedHours / (double)totalHours) * 100, 1) : 0;
        var placementStatus = placement.PlacementStatus.ToString();

        return new ProgressDto(
            student.StudentUUID,
            student.FullName,
            placement.Office?.OfficeName ?? string.Empty,
            totalHours,
            accumulatedHours,
            remainingHours,
            trainingHoursRendered,
            trainingHoursForWeek,
            progressPercent,
            placementStatus
        );
    }
}
