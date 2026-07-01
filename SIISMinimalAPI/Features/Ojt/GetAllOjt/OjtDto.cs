using System;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Ojt.GetAllOjt;

public class OjtDto
{
    public Guid OjtUUID { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; } = string.Empty;
    public OfficeNameEnum? OfficeName { get; set; }
    
    public GennderEnum Gender { get; set; }

    public DateOnly DateOfBirth {get; set;}
    public DateOnly? StartDate {get; set;}
    public DateOnly? EstimatedEndDate {get; set;}
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

}
