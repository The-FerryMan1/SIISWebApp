using System;

namespace SIISMinimalAPI.Features.Application;

public interface IApplicationService
{
    Task<ICollection<ApplicationDto>> GetAllAsync(CancellationToken ct);
}
