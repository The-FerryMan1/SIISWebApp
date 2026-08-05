using System;
using SIISMinimalAPI.Features.OnBoarding;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Offices.GetAllOffices;

public class GetAllOfficeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Department { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public List<StudentCount> Students { get; set; } = [];
}
 

public class StudentCount
{
    public long Id {get; set;}
}
