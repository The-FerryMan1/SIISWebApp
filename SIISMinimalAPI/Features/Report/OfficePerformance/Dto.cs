using System;

namespace SIISMinimalAPI.Features.Report.OfficePerformance;

public class OfficePerformanceDto
{
    public string? OfficeName { get; set; } = string.Empty;
    public int TotalOJTs { get; set; }
    public double AverageAccumulatedHours { get; set; }
    public double AverageCompletionRate { get; set; }
}
