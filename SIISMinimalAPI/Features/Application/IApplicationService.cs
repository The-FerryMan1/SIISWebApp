using System;
using SIISMinimalAPI.Features.Application.GetById;

namespace SIISMinimalAPI.Features.Application;

public interface IApplicationService
{
    Task<ICollection<ApplicationDto>> GetAllAsync(CancellationToken ct);
    Task<ApplicationGetByIdDto> GetByIdAsync(Guid uuid, CancellationToken ct);
}
