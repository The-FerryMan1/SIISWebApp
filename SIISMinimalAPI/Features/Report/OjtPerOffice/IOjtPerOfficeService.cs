using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Utilities;

namespace SIISMinimalAPI.Features.Report.OjtPerOffice;

public interface IOjtPerOfficeService
{
    Task<byte[]> ListAllOjtPerOffice(string office, CancellationToken ct);
    Task<byte[]> ListAllOjtPerOfficeFiltered(CommonFilterOptions filters, CancellationToken ct);
}
