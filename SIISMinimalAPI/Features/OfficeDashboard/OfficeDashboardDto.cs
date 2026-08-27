namespace SIISMinimalAPI.Features.OfficeDashboard;

public class OfficeDashboardDto
{
    public long OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int OngoingCount { get; set; }
    public int FinishedCount { get; set; }
    public List<StudentItemDto> Students { get; set; } = new();
}

public class StudentItemDto
{
    public Guid StudentUuid { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string School { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EstimatedEndDate { get; set; }
    public int TotalHours { get; set; }
    public int AccumulatedHours { get; set; }
    public double HoursProgress => TotalHours > 0 ? Math.Round((AccumulatedHours / (double)TotalHours) * 100, 1) : 0;
}

public class UpdateInternshipDatesDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EstimatedEndDate { get; set; }
    public int AccumulatedHours { get; set; }
}