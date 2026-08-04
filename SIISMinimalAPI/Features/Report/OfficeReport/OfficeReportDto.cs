using System;

namespace SIISMinimalAPI.Features.Report.OfficeReport;

public class OfficeReportDto
{
    public long OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
