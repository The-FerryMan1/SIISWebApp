using System;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Report.HoursProgress;

public interface IHoursProgressService
{
    Task<byte[]> GeneratePdf(CancellationToken ct);
    Task<byte[]> GenerateCsv(CancellationToken ct);
}
