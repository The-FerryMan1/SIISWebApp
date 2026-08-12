using System;

namespace SIISMinimalAPI.Features.Report.HoursProgress;

public class HoursProgressDto
{
    public string? StudentName { get; set; } = string.Empty;
    public string? Office { get; set; } = string.Empty;
    public int TotalHours { get; set; }
    public int AccumulatedHours { get; set; }
    public double ProgressPercent { get; set; }
}
