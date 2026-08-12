using System;

namespace SIISMinimalAPI.Features.Report.CompletionSummary;

public class CompletionSummaryDto
{
    public string? StudentName { get; set; } = string.Empty;
    public string? Office { get; set; } = string.Empty;
    public int TotalHours { get; set; }
    public int AccumulatedHours { get; set; }
    public string? CompletionDate { get; set; }
}
