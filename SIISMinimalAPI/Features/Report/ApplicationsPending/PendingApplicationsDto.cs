using System;

namespace SIISMinimalAPI.Features.Report.ApplicationsPending;

public class PendingApplicationsDto
{
    public string FullName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public string InternshipType { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
}
