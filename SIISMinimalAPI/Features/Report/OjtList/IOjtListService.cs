using System;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Report.OjtList;

public interface IOjtListService
{
    Task<byte[]> ListAllOjt(ApplicationStatusEnum status, CancellationToken ct);
    Task<byte[]> OjtListCsv(CancellationToken ct);   
}
