using System;

namespace SIISMinimalAPI.Features.Report.RequirementsCompliance;

public class RequirementsComplianceDto
{
    public string? StudentName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? School { get; set; } = string.Empty;
    public int TotalRequirements { get; set; }
    public int MissingCount { get; set; }
    public int ExpiredCount { get; set; }
}
