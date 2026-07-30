using System;

namespace SIISMinimalAPI.Features.Report.RequirementsChecklist;

public class RequirementsChecklistDto
{
    public string StudentName { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
