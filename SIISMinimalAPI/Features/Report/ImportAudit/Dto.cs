using System;

namespace SIISMinimalAPI.Features.Report.ImportAudit;

public class ImportAuditDto
{
    public string? Action { get; set; } = string.Empty;
    public string? Entity { get; set; } = string.Empty;
    public long? EntityId { get; set; }
    public string? UserId { get; set; } = string.Empty;
    public string? Details { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
}
