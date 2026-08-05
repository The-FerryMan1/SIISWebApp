using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Models;
using SIISMinimalAPI.Features.Offices.GetAllOffices;
using SIISMinimalAPI.Features.Offices.UpdateOffice;

namespace SIISMinimalAPI.Features.Offices;

public class OfficeHandler(AppDbContext context, UserManager<User> userManager) : IOfficeService
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;
    public async Task<ICollection<GetAllOfficeDto>>? GetallOfficeAsync(CancellationToken ct)
    {
        var offices = await _context.Offices.Include(t => t.Placements).AsSingleQuery().ToListAsync(ct);
        var dtos = new List<GetAllOfficeDto>();
        foreach (var o in offices)
        {
            string? userEmail = null;
            if (!string.IsNullOrEmpty(o.UserId))
            {
                var user = await _userManager.FindByIdAsync(o.UserId);
                userEmail = user?.Email;
            }

            dtos.Add(new GetAllOfficeDto
            {
                Id = o.Id,
                Name = o.OfficeName,
                UserId = o.UserId,
                UserEmail = userEmail,
                Department = o.Department,
                CreateAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                Students = [.. o.Placements.Select(s => new StudentCount
                {
                    Id = s.Id
                })]
            });
        }
        return dtos;
    }

    public async Task UpdateOfficeAsync(long id,UpdateOfficeDto dto, CancellationToken ct)
    {
        var exist = await _context.Offices.FirstOrDefaultAsync(t => t.Id == id, ct)
        ?? throw new KeyNotFoundException("Office not found");
        exist.OfficeName = dto.OfficeName;
        exist.Department = dto.Department;
        _context.Offices.Update(exist);
        await _context.SaveChangesAsync(ct);
    }
}
