using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.OjtList;

public interface IOjtListService
{
    Task<byte[]> ListAllOjt(ApplicationStatusEnum? status, CancellationToken ct);
    Task<byte[]> OjtListCsv(CancellationToken ct);
    Task<byte[]> ListAllOjtFiltered(CommonFilterOptions filters, CancellationToken ct);
    Task<byte[]> OjtListCsvFiltered(CommonFilterOptions filters, CancellationToken ct);
}
