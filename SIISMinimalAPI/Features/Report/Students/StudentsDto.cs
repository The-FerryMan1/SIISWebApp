using System;

namespace SIISMinimalAPI.Features.Report.Students;

public class StudentsDto
{
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
}
