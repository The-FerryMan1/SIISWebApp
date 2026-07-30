using System;

namespace SIISMinimalAPI.Features.Report.RequirementsChecklist;

public interface IRequirementsChecklistService
{
    Task<byte[]> GetRequirementsChecklist(CancellationToken ct);
    Task<byte[]> GetRequirementsChecklistCsv(CancellationToken ct);
}
