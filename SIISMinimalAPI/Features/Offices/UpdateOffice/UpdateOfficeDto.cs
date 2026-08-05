using System;

namespace SIISMinimalAPI.Features.Offices.UpdateOffice;

public class UpdateOfficeDto
{
    public string OfficeName { get; set; } = string.Empty;
    public string? Department { get; set; }
}
