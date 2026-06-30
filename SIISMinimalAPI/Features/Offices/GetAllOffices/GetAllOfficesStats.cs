using System;

namespace SIISMinimalAPI.Features.Offices.GetAllOffices;

public class GetAllOfficesStats
{
    public ICollection<GetAllOfficeDto> Offices { get; set; } = [];
    
}
