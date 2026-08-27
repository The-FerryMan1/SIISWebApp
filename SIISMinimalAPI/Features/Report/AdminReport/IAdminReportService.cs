using System;
using System.Threading;
using System.Threading.Tasks;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.AdminReport;

public interface IAdminReportService
{
    Task<byte[]> GenerateExpiringInternshipsPdf(long? officeId, int days, string? school, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct);
    Task<byte[]> GenerateMasterlistPdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateMasterlistCsv(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateOngoingPdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateOngoingCsv(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateFinishedPdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateFinishedCsv(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateRejectedPdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateRejectedCsv(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateApprovedPdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateApprovedCsv(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GeneratePendingPdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GeneratePendingCsv(CommonFilterOptions filters, CancellationToken ct);
}
