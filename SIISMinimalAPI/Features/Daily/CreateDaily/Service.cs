using SIISMinimalAPI.Data;
using SharedModels = SIISMinimalAPI.Features.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace SIISMinimalAPI.Features.Daily.CreateDaily;

public class Service(AppDbContext db) : IService
{   
    private readonly AppDbContext _db = db;
    public async Task<Request> CreateDailyAsync(Request request, CancellationToken ct)
    {
        if (request.Dailies == null || !request.Dailies.Any())
            return request;

        var firstDate = request.Dailies.Min(d => d.Date);
        var weekStart = GetWeekStart(firstDate);
        var weekEnd = weekStart.AddDays(6);

        var student = await _db.Students
            .Include(s => s.Placement)
            .ThenInclude(p => p.Progress)
            .FirstOrDefaultAsync(s => s.Placement != null && s.Placement.Progress != null, ct);

        if (student == null)
            throw new KeyNotFoundException("Student or placement not found");

        var progress = student.Placement!.Progress!;

        var weeklyReport = await _db.WeeklyReports
            .Include(w => w.DailyReport)
            .FirstOrDefaultAsync(w => w.ProgressId == progress.Id && w.WeekStartDate == weekStart && w.WeekEndDate == weekEnd, ct);

        if (weeklyReport == null)
        {
            weeklyReport = new SharedModels.WeeklyReport
            {
                ProgressId = progress.Id,
                WeekStartDate = weekStart,
                WeekEndDate = weekEnd,
                DailyReport = new List<SharedModels.DailyReport>()
            };
            _db.WeeklyReports.Add(weeklyReport);
            await _db.SaveChangesAsync(ct);
        }

        foreach (var daily in request.Dailies)
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
        return request;
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }
}