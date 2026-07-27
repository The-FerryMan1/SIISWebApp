using System;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.OjtPerOffice;

public interface IOjtPerOfficeService
{
    Task<byte[]> ListAllOjtPerOffice(OfficeNameEnum office, CancellationToken ct);
}
