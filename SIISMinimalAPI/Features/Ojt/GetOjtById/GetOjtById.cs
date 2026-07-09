using System;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Ojt.GetOjtById;

public class GetOjtById
{
    public Guid StudentUUID { get; set; }
    public string Email { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public OfficeNameEnum Office {get; set;}
    public DateOnly DateOfBirth { get; set; }
    public GennderEnum Gender { get; set; }
    public GradeLevelEnum GradeLevel { get; set; }
}
