namespace SIISMinimalAPI.Features.WeeklyReport.WeeklyReportCrud;

public record WeeklyReportDto
{
    public int Id { get; set; }
    public DateOnly WeekStartDate { get; set; }
    public DateOnly WeekEndDate { get; set; }
    public int TotalHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DailyReportDto> DailyReports { get; set; } = new();
}

public record DailyReportDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string Activities { get; set; } = string.Empty;
    public int Hours { get; set; }
    public string? InCharge { get; set; }
    public string? Remarks { get; set; }
    public string? IncidentReport { get; set; }
}

public record CreateWeeklyReportRequest
{
    public required Guid StudentUuid { get; set; }
    public required DateOnly WeekStartDate { get; set; }
    public required List<DailyReportDto> DailyReports { get; set; }
}

public record UpdateWeeklyReportRequest
{
    public List<DailyReportDto> DailyReports { get; set; } = new();
}