using System;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Report.OfficeReport;

public interface IOfficeReportService
{
    Task<byte[]> GenerateMasterlistPdf(long officeId, CancellationToken ct);
    Task<byte[]> GenerateExpiringPdf(long officeId, CancellationToken ct);
    Task<byte[]> GenerateFinishedPdf(long officeId, CancellationToken ct);
}
