using System;

namespace SIISMinimalAPI.Features.Report.StudentMasterlist;

public class StudentMasterlistDto
{
    public string? Name { get; set; } = string.Empty;
    public string? Office { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
    public string? GradeLevel { get; set; } = string.Empty;
    public string? Strand { get; set; } = string.Empty;
    public string? Degree { get; set; } = string.Empty;
    public int TotalHours { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public int AccumulatedHours { get; set; }
}
