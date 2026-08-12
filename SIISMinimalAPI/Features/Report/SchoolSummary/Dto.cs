using System;

namespace SIISMinimalAPI.Features.Report.SchoolSummary;

public class SchoolSummaryDto
{
    public string? SchoolName { get; set; } = string.Empty;
    public int TotalApplicants { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Pending { get; set; }
}
