using System;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Ojt.GetAllOjt;

namespace SIISMinimalAPI.Features.Ojt;

public class OjtHandler(AppDbContext context) : IOjtService
{
    private readonly AppDbContext _context = context;
    public async Task<ICollection<OjtDto>>? GetAllOjtAsync(CancellationToken ct)
    {
        var ojts = await _context.Students
        .Include(t => t.Application)
        .Include(t => t.Internship)
        .Include(t => t.School)
        .Include(t => t.Office)
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
            OfficeName = t.Office.Name,
            DateOfBirth = t.DateOfBirth,
            UniversitySchool = t.School.Name,
            EstimatedEndDate = t.Internship.EstimatedEndDate,
            StartDate = t.Internship.StartDate,
            Gender = t.Gender,
            CreatedAt = t.CreateAt,
            UpdatedAt = t.UpdatedAt,
        }).ToList();


    }
}
