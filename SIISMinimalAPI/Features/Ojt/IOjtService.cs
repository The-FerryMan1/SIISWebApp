using System;
using SIISMinimalAPI.Features.Ojt.GetAllOjt;

namespace SIISMinimalAPI.Features.Ojt;

public interface IOjtService
{
    Task<ICollection<OjtDto>> GetAllOjtAsync(CancellationToken ct);
    Task<GetOjtById.GetOjtById>? GetOjtById(Guid guid, CancellationToken ct);
    Task DeleteOjt(Guid guid, CancellationToken ct);
}
