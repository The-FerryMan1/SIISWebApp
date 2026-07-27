using System;

namespace SIISMinimalAPI.Features.Report.OjtList;

public class OjtListDto
{
    public string? Name { get; set; } = string.Empty;
    public string? Office { get; set; } = string.Empty;

    public string? Status {get; set;} = string.Empty;
    public DateOnly? StartDate { get; set; }
    public int TotalHours { get; set; }
}
