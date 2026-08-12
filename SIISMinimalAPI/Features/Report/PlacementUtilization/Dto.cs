using System;

namespace SIISMinimalAPI.Features.Report.PlacementUtilization;

public class PlacementUtilizationDto
{
    public string? OfficeName { get; set; } = string.Empty;
    public int CurrentAssigned { get; set; }
}
