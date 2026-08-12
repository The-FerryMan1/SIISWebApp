using System;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Logs;
using SIISMinimalAPI.Features.Ojt.GetAllOjt;

namespace SIISMinimalAPI.Features.Ojt;

public class OjtHandler(AppDbContext context, ILogService logService) : IOjtService
{
    private readonly AppDbContext _context = context;
    private readonly ILogService _logService = logService;

    public async Task DeleteOjt(Guid guid, CancellationToken ct)
    {
       var ojt = await _context.Students.FirstOrDefaultAsync(t => t.StudentUUID == guid, ct)
       ?? throw new KeyNotFoundException("Student not found");

       _context.Remove(ojt);
       await _context.SaveChangesAsync(ct);

       var deleteUserId = context.Entry(ojt).Property("Id").CurrentValue.ToString() ?? "unknown";
       await _logService.WriteAsync("Delete", "OJT", ojt.Id, deleteUserId, $"Deleted OJT {ojt.FullName}");
    }

    public async Task<ICollection<OjtDto>>? GetAllOjtAsync(CancellationToken ct)
    {
        var ojts = await _context.Students
        .Include(t => t.Application)
        .Include(t => t.Placement).ThenInclude(p => p.Office)
        .Where(t => t.Application.Status == Shared.Enums.ApplicationStatusEnum.Approved)
        .AsNoTracking()
        .AsSplitQuery()
        .ToListAsync(ct);

        return ojts.Select(t => new OjtDto
        {
            OjtUUID = t.StudentUUID,
            LastName = t.LastName,
            FirstName = t.FirstName,
            MiddleName = t.MiddleName,
            OfficeName = t.Placement?.Office?.OfficeName ?? string.Empty,
            DateOfBirth = t.DateOfBirth,
            UniversitySchool = t.SchoolName,
            EstimatedEndDate = t.Placement?.EstimatedEndDate,
            StartDate = t.Placement?.StartDate,
            Gender = t.Gender,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
        }).ToList();


    }

    public async Task<GetOjtById.GetOjtById>? GetOjtById(Guid guid, CancellationToken ct)
    {
        var ojt = await _context.Students.Include(t => t.Placement).ThenInclude(p => p.Office).FirstOrDefaultAsync(t => t.StudentUUID == guid, ct)
        ?? throw new KeyNotFoundException("User not found");

        return new GetOjtById.GetOjtById
        {
          StudentUUID = ojt.StudentUUID,
          Address = ojt.Address,
          ContactNumber = ojt.ContactNumber,
          DateOfBirth = ojt.DateOfBirth,
          Email = ojt.Email,
          FirstName = ojt.FirstName,
          Gender = ojt.Gender,
          GradeLevel = ojt.GradeLevel,
          LastName = ojt.LastName,
          MiddleName = ojt.MiddleName,
          Office = ojt.Placement?.Office?.OfficeName ?? string.Empty
        };
    }
}
