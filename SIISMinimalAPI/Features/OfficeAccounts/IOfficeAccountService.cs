using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.OfficeAccounts;

public interface IOfficeAccountService
{
    Task<ICollection<GetOfficeAccountDto>> GetAllAsync(CancellationToken ct);
    Task<GetOfficeAccountDto> GetByIdAsync(long id, CancellationToken ct);
    Task CreateAsync(CreateOfficeAccountDto dto, CancellationToken ct);
    Task UpdateAsync(long id, UpdateOfficeAccountDto dto, CancellationToken ct);
    Task DeleteAsync(long id, CancellationToken ct);
}