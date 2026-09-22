using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SharedModels = SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.WeeklyReport.WeeklyReportCrud;

public interface IWeeklyReportCrudService
{
    Task<List<WeeklyReportDto>> GetWeeklyReportsByStudentAsync(Guid studentUuid, CancellationToken ct);
    Task<WeeklyReportDto?> GetWeeklyReportAsync(int weeklyReportId, CancellationToken ct);
    Task<WeeklyReportDto> CreateWeeklyReportAsync(CreateWeeklyReportRequest request, CancellationToken ct);
    Task<WeeklyReportDto> UpdateWeeklyReportAsync(int weeklyReportId, UpdateWeeklyReportRequest request, CancellationToken ct);
    Task DeleteWeeklyReportAsync(int weeklyReportId, CancellationToken ct);
}

public class WeeklyReportCrudService(AppDbContext db) : IWeeklyReportCrudService
{
    private readonly AppDbContext _db = db;

    public async Task<List<WeeklyReportDto>> GetWeeklyReportsByStudentAsync(Guid studentUuid, CancellationToken ct)
    {
        var student = await _db.Students
            .Include(s => s.Placement)
            .ThenInclude(p => p.Progress)
            .ThenInclude(p => p.WeeklyReports)
            .ThenInclude(w => w.DailyReport)
            .FirstOrDefaultAsync(s => s.StudentUUID == studentUuid && !s.IsDeleted, ct);

        if (student?.Placement?.Progress == null)
            return new List<WeeklyReportDto>();

        return student.Placement.Progress.WeeklyReports?
            .OrderByDescending(w => w.WeekStartDate)
            .Select(w => new WeeklyReportDto
            {
                Id = w.Id,
                WeekStartDate = w.WeekStartDate,
                WeekEndDate = w.WeekEndDate,
                TotalHours = w.DailyReport?.Sum(d => d.Hours) ?? 0,
                CreatedAt = w.DailyReport?.Min(d => d.CreatedAt) ?? DateTime.MinValue,
                DailyReports = w.DailyReport?
                    .OrderBy(d => d.Date)
                    .Select(d => new DailyReportDto
                    {
                        Id = d.Id,
                        Date = d.Date,
                        Activities = d.Activities,
                        Hours = d.Hours,
                        InCharge = d.InCharge,
                        Remarks = d.Remarks,
                        IncidentReport = d.IncidentReport
                    }).ToList() ?? new List<DailyReportDto>()
            }).ToList() ?? new List<WeeklyReportDto>();
    }

    public async Task<WeeklyReportDto?> GetWeeklyReportAsync(int weeklyReportId, CancellationToken ct)
    {
        var weeklyReport = await _db.WeeklyReports
            .Include(w => w.DailyReport)
            .FirstOrDefaultAsync(w => w.Id == weeklyReportId, ct);

        if (weeklyReport == null)
            return null;

        return new WeeklyReportDto
        {
            Id = weeklyReport.Id,
            WeekStartDate = weeklyReport.WeekStartDate,
            WeekEndDate = weeklyReport.WeekEndDate,
            TotalHours = weeklyReport.DailyReport?.Sum(d => d.Hours) ?? 0,
            CreatedAt = weeklyReport.DailyReport?.Min(d => d.CreatedAt) ?? DateTime.MinValue,
            DailyReports = weeklyReport.DailyReport?
                .OrderBy(d => d.Date)
                .Select(d => new DailyReportDto
                {
                    Id = d.Id,
                    Date = d.Date,
                    Activities = d.Activities,
                    Hours = d.Hours,
                    InCharge = d.InCharge,
                    Remarks = d.Remarks,
                    IncidentReport = d.IncidentReport
                }).ToList() ?? new List<DailyReportDto>()
        };
    }

    public async Task<WeeklyReportDto> CreateWeeklyReportAsync(CreateWeeklyReportRequest request, CancellationToken ct)
    {
        var student = await _db.Students
            .Include(s => s.Placement)
            .ThenInclude(p => p.Progress)
            .FirstOrDefaultAsync(s => s.StudentUUID == request.StudentUuid && !s.IsDeleted, ct);

        if (student?.Placement?.Progress == null)
            throw new KeyNotFoundException("Student, placement, or progress not found");

        var progress = student.Placement.Progress;

        var existing = await _db.WeeklyReports
            .FirstOrDefaultAsync(w => w.ProgressId == progress.Id && w.WeekStartDate == request.WeekStartDate, ct);

        if (existing != null)
            throw new InvalidOperationException("Weekly report already exists for this week");

        var weekEnd = request.WeekStartDate.AddDays(6);

        var weeklyReport = new SharedModels.WeeklyReport
        {
            ProgressId = progress.Id,
            WeekStartDate = request.WeekStartDate,
            WeekEndDate = weekEnd,
            DailyReport = new List<SharedModels.DailyReport>()
        };

        _db.WeeklyReports.Add(weeklyReport);
        await _db.SaveChangesAsync(ct);

        foreach (var daily in request.DailyReports)
        {
            var dailyReport = new SharedModels.DailyReport
            {
                Date = daily.Date,
                Activities = daily.Activities,
                Hours = daily.Hours,
                InCharge = daily.InCharge,
                Remarks = daily.Remarks,
                IncidentReport = daily.IncidentReport,
                WeeklyReportId = weeklyReport.Id
            };
            _db.DailyReports.Add(dailyReport);
        }

        await _db.SaveChangesAsync(ct);

        return await GetWeeklyReportAsync(weeklyReport.Id, ct) ?? throw new InvalidOperationException("Failed to retrieve created weekly report");
    }

    public async Task<WeeklyReportDto> UpdateWeeklyReportAsync(int weeklyReportId, UpdateWeeklyReportRequest request, CancellationToken ct)
    {
        var weeklyReport = await _db.WeeklyReports
            .Include(w => w.DailyReport)
            .FirstOrDefaultAsync(w => w.Id == weeklyReportId, ct);

        if (weeklyReport == null)
            throw new KeyNotFoundException("Weekly report not found");

        if (request.DailyReports != null && request.DailyReports.Any())
        {
            _db.DailyReports.RemoveRange(weeklyReport.DailyReport);
            weeklyReport.DailyReport.Clear();

            foreach (var daily in request.DailyReports)
            {
                var dailyReport = new SharedModels.DailyReport
                {
                    Date = daily.Date,
                    Activities = daily.Activities,
                    Hours = daily.Hours,
                    InCharge = daily.InCharge,
                    Remarks = daily.Remarks,
                    IncidentReport = daily.IncidentReport,
                    WeeklyReportId = weeklyReport.Id
                };
                _db.DailyReports.Add(dailyReport);
            }
        }

        await _db.SaveChangesAsync(ct);
        return await GetWeeklyReportAsync(weeklyReportId, ct) ?? throw new InvalidOperationException("Failed to retrieve updated weekly report");
    }

    public async Task DeleteWeeklyReportAsync(int weeklyReportId, CancellationToken ct)
    {
        var weeklyReport = await _db.WeeklyReports
            .Include(w => w.DailyReport)
            .FirstOrDefaultAsync(w => w.Id == weeklyReportId, ct);

        if (weeklyReport == null)
            throw new KeyNotFoundException("Weekly report not found");

        if (weeklyReport.DailyReport != null && weeklyReport.DailyReport.Any())
        {
            _db.DailyReports.RemoveRange(weeklyReport.DailyReport);
        }

        _db.WeeklyReports.Remove(weeklyReport);
        await _db.SaveChangesAsync(ct);
    }
}