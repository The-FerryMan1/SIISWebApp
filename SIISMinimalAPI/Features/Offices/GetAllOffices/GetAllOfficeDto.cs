using System;
using SIISMinimalAPI.Features.OnBoarding;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Offices.GetAllOffices;

public class GetAllOfficeDto
{
    public long Id { get; set; }
    public OfficeNameEnum Name { get; set; }
    public string? CurrentOIC { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public List<StudentCount> Students { get; set; } = [];
}
 

public class StudentCount
{
    public long Id {get; set;}
}
