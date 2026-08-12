using System;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Report.RequirementsCompliance;

public interface IRequirementsComplianceService
{
    Task<byte[]> GeneratePdf(CancellationToken ct);
    Task<byte[]> GenerateCsv(CancellationToken ct);
}
