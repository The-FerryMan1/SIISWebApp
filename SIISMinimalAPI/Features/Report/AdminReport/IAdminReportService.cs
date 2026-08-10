using System;
using System.Threading;
using System.Threading.Tasks;

namespace SIISMinimalAPI.Features.Report.AdminReport;

public interface IAdminReportService
{
    Task<byte[]> GenerateExpiringInternshipsPdf(long? officeId, int days, CancellationToken ct);
}
