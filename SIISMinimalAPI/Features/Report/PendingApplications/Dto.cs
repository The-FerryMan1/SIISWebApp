using System;

namespace SIISMinimalAPI.Features.Report.PendingApplications;

public class PendingApplicationsDto
{
    public string? Name { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? School { get; set; } = string.Empty;
    public DateTime? SubmissionDate { get; set; }
    public string? Status { get; set; } = string.Empty;
}
