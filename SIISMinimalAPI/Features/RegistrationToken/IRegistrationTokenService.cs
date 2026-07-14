using System;
using System.Collections.ObjectModel;

namespace SIISMinimalAPI.Features.RegistrationToken;

public interface IRegistrationTokenService
{
    Task CreateRegistrationToken(GenerateRegistrationTokenDto dto, CancellationToken ct);
    Task<ICollection<RegistrationTokenDto>>? GetAllRegistrationToken(CancellationToken ct);
    Task<RegistrationTokenDto>? GetByIdRegistrationToken(long id, CancellationToken ct);
    Task ExtendRegistrationToken(long id, GenerateRegistrationTokenDto dot ,CancellationToken ct);
    Task DeleteRegistrationToken(long id, CancellationToken ct);
    Task<bool> VerifyRegistrationToken(Guid guid, CancellationToken ct);
    
}
