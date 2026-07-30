using System;

namespace SIISMinimalAPI.Features.Report.OfficesSummary;

public class OfficesSummaryDto
{
    public string Office { get; set; } = string.Empty;
    public string OIC { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public double SharePercentage { get; set; }
}
