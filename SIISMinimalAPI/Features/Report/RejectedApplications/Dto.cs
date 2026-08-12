using System;

namespace SIISMinimalAPI.Features.Report.RejectedApplications;

public class RejectedApplicationsDto
{
    public string? Name { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? School { get; set; } = string.Empty;
    public DateTime? SubmissionDate { get; set; }
    public string? Reason { get; set; } = string.Empty;
}
