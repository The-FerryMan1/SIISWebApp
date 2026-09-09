using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.WeeklyReport;

public class WeeklyReportDto
{
    public long Id { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public string ReportPeriod { get; set; } = string.Empty;
    public int TotalActiveStudents { get; set; }
    public int OngoingCount { get; set; }
    public int FinishedCount { get; set; }
    public int TotalHoursRenderedThisWeek { get; set; }
    public List<WeeklyStudentDto> Students { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}

public class WeeklyStudentDto
{
    public string FullName { get; set; } = string.Empty;
    public string School { get; set; } = string.Empty;
    public string PlacementStatus { get; set; } = string.Empty;
    public int TotalHours { get; set; }
    public int AccumulatedHours { get; set; }
    public int HoursThisWeek { get; set; }
    public double ProgressPercent { get; set; }
}

public class WeeklyHistoryDto
{
    public long Id { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public string ReportPeriod { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int TotalHoursThisWeek { get; set; }
    public DateTime GeneratedAt { get; set; }
}
