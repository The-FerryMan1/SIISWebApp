using System;

namespace SIISMinimalAPI.Features.Report.InternshipHours;

public class InternshipHoursDto
{
    public string FullName { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EstimatedEndDate { get; set; }
    public int TotalHours { get; set; }
    public string Status { get; set; } = string.Empty;
}
