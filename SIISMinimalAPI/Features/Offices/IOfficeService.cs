using System;
using SIISMinimalAPI.Features.Offices.GetAllOffices;

namespace SIISMinimalAPI.Features.Offices;

public interface IOfficeService
{
    Task<ICollection<GetAllOfficeDto>>? GetallOfficeAsync(CancellationToken ct); 
}
