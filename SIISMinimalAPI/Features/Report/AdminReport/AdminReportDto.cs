using System;

namespace SIISMinimalAPI.Features.Report.AdminReport;

public class AdminReportMasterlistDto
{
    public string? Name { get; set; } = string.Empty;
    public string? School { get; set; } = string.Empty;
    public string? PlacementStatus { get; set; } = string.Empty;
    public string? OfficeAssigned { get; set; } = string.Empty;
}

public class AdminReportOngoingDto
{
    public string? Name { get; set; } = string.Empty;
    public string? School { get; set; } = string.Empty;
    public int TotalInternshipHours { get; set; }
    public int AccumulatedHours { get; set; }
}

public class AdminReportFinishedDto
{
    public string? Name { get; set; } = string.Empty;
    public string? School { get; set; } = string.Empty;
}

public class AdminReportRejectedDto
{
    public string? Name { get; set; } = string.Empty;
    public string? School { get; set; } = string.Empty;
    public string? Reason { get; set; } = string.Empty;
}

public class AdminReportApprovedDto
{
    public string? Name { get; set; } = string.Empty;
    public string? School { get; set; } = string.Empty;
    public string? OfficeAssigned { get; set; } = string.Empty;
}

public class AdminReportPendingDto
{
    public string? Name { get; set; } = string.Empty;
    public string? School { get; set; } = string.Empty;
}
