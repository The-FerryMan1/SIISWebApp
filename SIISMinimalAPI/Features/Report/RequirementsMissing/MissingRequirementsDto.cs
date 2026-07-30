using System;

namespace SIISMinimalAPI.Features.Report.RequirementsMissing;

public class MissingRequirementsDto
{
    public string FullName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
}
