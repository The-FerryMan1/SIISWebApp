using System;

namespace SIISMinimalAPI.Features.Offices.UpdateOffice;

public class UpdateOfficeDto
{
    public string? Department { get; set; } = string.Empty;
    public string Honorific { get; set; }
}
