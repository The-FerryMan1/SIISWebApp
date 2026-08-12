using System;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Report.SchoolSummary;

public interface ISchoolSummaryService
{
    Task<byte[]> GeneratePdf(CancellationToken ct);
    Task<byte[]> GenerateCsv(CancellationToken ct);
}
