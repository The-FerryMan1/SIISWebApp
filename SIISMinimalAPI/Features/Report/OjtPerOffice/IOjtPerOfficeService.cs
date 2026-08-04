using System;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.OjtPerOffice;

public interface IOjtPerOfficeService
{
    Task<byte[]> ListAllOjtPerOffice(string office, CancellationToken ct);
    Task<byte[]> ListAllOjtPerOfficeFiltered(string? office, ApplicationStatusEnum? status, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct);
}
