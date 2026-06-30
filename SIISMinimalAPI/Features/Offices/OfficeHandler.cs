using System;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Offices.GetAllOffices;
using SIISMinimalAPI.Features.OnBoarding;

namespace SIISMinimalAPI.Features.Offices;

public class OfficeHandler(AppDbContext context) : IOfficeService
{
    private readonly AppDbContext _context = context;
    public async Task<ICollection<GetAllOfficeDto>>? GetallOfficeAsync(CancellationToken ct)
    {
        var offices = await _context.Offices.Include(t => t.Students).AsSingleQuery().ToListAsync(ct);
       return offices.Select(o => new GetAllOfficeDto
    {
        Id = o.Id,
        Name = o.Name,
        CurrentOIC = o.CurrentOIC,
        CreateAt = o.CreateAt,
        UpdatedAt = o.UpdatedAt,
        Students = [.. o.Students.Select(s => new StudentCount
        {
           Id = s.Id
        })]
       }).ToList();
    }
}
