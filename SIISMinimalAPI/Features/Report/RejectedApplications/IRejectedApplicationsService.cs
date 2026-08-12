using System;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Report.RejectedApplications;

public interface IRejectedApplicationsService
{
    Task<byte[]> GeneratePdf(CancellationToken ct);
    Task<byte[]> GenerateCsv(CancellationToken ct);
}
