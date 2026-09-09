using SIISMinimalAPI.Features.Report.WeeklyReport;

using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.WeeklyReport;

public interface IWeeklyReportService
{
    Task<byte[]> GenerateWeeklyPdf(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> GenerateWeeklyCsv(CommonFilterOptions filters, CancellationToken ct);
    Task<List<WeeklyHistoryDto>> GetWeeklyHistory(string? officeName, CancellationToken ct);
}
