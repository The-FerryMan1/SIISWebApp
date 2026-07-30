using System;

namespace SIISMinimalAPI.Features.Report.InternshipExpiring;

public class InternshipExpiringDto
{
    public string FullName { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public DateOnly EstimatedEndDate { get; set; }
    public int DaysLeft { get; set; }
    public int TotalHours { get; set; }
}
