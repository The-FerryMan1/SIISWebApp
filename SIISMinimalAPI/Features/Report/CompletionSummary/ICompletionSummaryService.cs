using System;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Report.CompletionSummary;

public interface ICompletionSummaryService
{
    Task<byte[]> GeneratePdf(CancellationToken ct);
    Task<byte[]> GenerateCsv(CancellationToken ct);
}
