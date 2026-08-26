using SIISMinimalAPI.Features.Shared.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Report.CompletionSummary;

public interface ICompletionSummaryService
{
    Task<byte[]> GeneratePdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateCsv(CommonFilterOptions filters, CancellationToken ct);
}
