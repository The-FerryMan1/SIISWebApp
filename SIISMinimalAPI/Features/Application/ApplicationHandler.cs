using System;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;

namespace SIISMinimalAPI.Features.Application;

public class ApplicationHandler(AppDbContext context) : IApplicationService
{
    private readonly AppDbContext _context = context;
    public async Task<ICollection<ApplicationDto>> GetAllAsync(CancellationToken ct)
    {
        var applications = await _context.Students
        .Include(t => t.Application).AsSplitQuery()
        .AsNoTracking().ToListAsync();

        return applications.Select(t => new ApplicationDto
        {
            Id = t.Application.Id,
            ApplicationUUID = t.Application.ApplicationUUID,
            FullName = $"{t.LastName}, {t.FirstName} {t.MiddleName}".Trim(),  
            Status = t.Application.Status.ToString(),                          
            CreatedAt = t.Application.CreateAt,
            UpdatedAt = t.Application.UpdatedAt                                
        }).ToList();
    }
}
