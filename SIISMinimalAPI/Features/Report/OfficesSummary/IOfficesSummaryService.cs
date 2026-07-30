using System;

namespace SIISMinimalAPI.Features.Report.OfficesSummary;

public interface IOfficesSummaryService
{
    Task<byte[]> GetOfficesSummary(CancellationToken ct);
}
