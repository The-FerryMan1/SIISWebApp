using System;
using SIISMinimalAPI.Features.Application.AssignAndApprove;
using SIISMinimalAPI.Features.Application.GetById;

namespace SIISMinimalAPI.Features.Application;

public interface IApplicationService
{
    Task<ICollection<ApplicationDto>> GetAllAsync(CancellationToken ct);
    Task<ApplicationGetByIdDto> GetByIdAsync(Guid uuid, CancellationToken ct);
    Task AssignAndApprove(Guid uuid, RequestDto requestDto, CancellationToken ct);
    Task Trash(Guid uuid, CancellationToken ct);
    Task DeleteAsync(Guid uuid, CancellationToken ct);
}
