using System;
using SIISMinimalAPI.Features.Offices.GetAllOffices;
using SIISMinimalAPI.Features.Offices.UpdateOffice;

namespace SIISMinimalAPI.Features.Offices;

public interface IOfficeService
{
    Task<ICollection<GetAllOfficeDto>>? GetallOfficeAsync(CancellationToken ct); 
    Task UpdateOfficeAsync(long id, UpdateOfficeDto dto, CancellationToken ct);
}
