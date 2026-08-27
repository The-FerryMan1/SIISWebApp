using System;
using System.Threading;
using System.Threading.Tasks;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.OfficeReport;

public interface IOfficeReportService
{
    Task<byte[]> GenerateMasterlistPdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateOngoingPdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateFinishedPdf(CommonFilterOptions filters, CancellationToken ct);
}
