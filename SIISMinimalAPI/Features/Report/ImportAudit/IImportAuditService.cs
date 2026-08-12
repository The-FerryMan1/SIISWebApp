using System;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Report.ImportAudit;

public interface IImportAuditService
{
    Task<byte[]> GenerateCsv(CancellationToken ct);
}
