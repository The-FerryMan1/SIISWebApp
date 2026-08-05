
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Application.AssignAndApprove;
using SIISMinimalAPI.Features.Application.GetById;

namespace SIISMinimalAPI.Features.Application;

public class ApplicationHandler(AppDbContext context) : IApplicationService
{
    private readonly AppDbContext _context = context;

    public async Task AssignAndApprove(Guid uuid, RequestDto requestDto, CancellationToken ct)
    {
        var exists = await _context.Students
     .Include(t => t.Application)
     .Include(t => t.Placement)
     .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, ct)
     ?? throw new KeyNotFoundException("Application not found");

        var office = await _context.Offices
            .FirstOrDefaultAsync(t => t.OfficeName == requestDto.Office, ct)
            ?? throw new KeyNotFoundException("No office found");


        if (exists.Placement == null)
        {
            exists.Placement = new Shared.Models.Placement
            {
                OfficeId = office.Id,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EstimatedEndDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(3)),
                AccumulatedHours = 0
            };
        }
        else
        {
            exists.Placement.OfficeId = office.Id;
        }

        exists.Application.Status = Shared.Enums.ApplicationStatusEnum.Approved;
        exists.Application.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct)
    {
        var application = await _context.Students
        .Include(t => t.Application)
        .Include(t => t.Requirements)
        .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, ct)
        ?? throw new KeyNotFoundException("Application not found");


        foreach (var req in application.Requirements)
        {
            if (!string.IsNullOrEmpty(req.FilePath) && File.Exists(req.FilePath))
            {
                File.Delete(req.FilePath);
            }
        }

        _context.Remove(application);
        await _context.SaveChangesAsync(ct);

    }

    public async Task<ICollection<ApplicationDto>> GetAllAsync(CancellationToken ct)
    {
        var applications = await _context.Students
        .Include(t => t.Application)
        .Include(t => t.Placement).ThenInclude(p => p.Office)
        .AsSplitQuery()
        .AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken: ct);

        return [.. applications.Select(t => {

           var degreeStrand = t.Degree.ToString()
                ?? t.Strand.ToString();

            return new ApplicationDto
        {
            Id = t.Application.Id,
            ApplicationUUID = t.Application.ApplicationUUID,
            FullName = t.FullName,
            Status = t.Application.Status.ToString(),
            DegreeStrand = degreeStrand,
            SchoolName = t.SchoolName,
            OfficeName = t.Placement?.Office?.OfficeName,
            CreatedAt = t.Application.CreatedAt,
            UpdatedAt = t.Application.UpdatedAt
        };
        })];
    }

    public async Task<ApplicationGetByIdDto> GetByIdAsync(Guid uuid, CancellationToken ct)
    {
        var application = await _context.Students
         .Include(t => t.Requirements)
         .Include(t => t.Application)
         .Include(t => t.Placement).ThenInclude(p => p.Office)
         .AsSplitQuery()
         .AsNoTracking()
         .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, cancellationToken: ct);

        if (application is null) throw new KeyNotFoundException("application not found");

        return new ApplicationGetByIdDto
        {
            Student = application is null ? null : new StudentInfo
            {
                Id = application.Id,
                StudentUUID = application.StudentUUID,
                Email = application.Email,
                LastName = application.LastName,
                FirstName = application.FirstName,
                MiddleName = application.MiddleName,
                ContactNumber = application.ContactNumber,
                Address = application.Address,
                DateOfBirth = application.DateOfBirth,
                Gender = application.Gender,
                GradeLevel = application.GradeLevel,
                SchoolName = application.SchoolName,
                SchoolAddress = application.SchoolAddress,
                SchoolContactPerson = application.SchoolContactPerson,
                SchoolContactPersonEmail = application.SchoolContactPersonEmail,
                SchoolContactPersonPhone = application.SchoolContactPersonPhone,
                InternshipNature = application.InternshipNature,
                Strand = application.Strand,
                Degree = application.Degree,
                TotalInternshipHours = application.TotalInternshipHours,
                IsDeleted = application.IsDeleted,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                DeletedAt = application.DeletedAt,
                OfficeId = application.Placement?.OfficeId
            },

            Application = new ApplicationInfo
            {
                Id = application.Application.Id,
                ApplicationUUID = application.Application.ApplicationUUID,
                Status = application.Application.Status,
                IsDeleted = application.Application.IsDeleted,
                CreatedAt = application.Application.CreatedAt,
                UpdatedAt = application.Application.UpdatedAt,
                DeletedAt = application.Application.DeletedAt
            },
            School = new SchoolInfo
            {
                Name = application.SchoolName,
                Address = application.SchoolAddress,
                ContactPerson = application.SchoolContactPerson,
                Email = application.SchoolContactPersonEmail,
                ContactNumber = application.SchoolContactPersonPhone
            },
            Internship = new InternshipInfo
            {
                InternshipNature = application.InternshipNature,
                Strand = application.Strand,
                Degree = application.Degree,
                InternshipTotalHours = application.TotalInternshipHours
            },
            Placement = application.Placement is not null ? new PlacementInfo
            {
                Id = application.Placement.Id,
                StartDate = application.Placement.StartDate,
                EstimatedEndDate = application.Placement.EstimatedEndDate,
                AccumulatedHours = application.Placement.AccumulatedHours,
                OfficeId = application.Placement.OfficeId,
                OfficeName = application.Placement.Office?.OfficeName ?? string.Empty,
                StudentId = application.Placement.StudentId
            } : null,
            Requirements = application.Requirements?
         .Where(r => !r.IsDeleted)
         .Select(t => new RequirementInfo
         {
             Id = t.Id,
             FileName = t.FileName,
             FilePath = t.FilePath,
             FileType = t.FileType,
             IsDeleted = t.IsDeleted,
             CreatedAt = t.CreatedAt,
             UpdatedAt = t.UpdatedAt,
             DeletedAt = t.DeletedAt
         }).ToList(),
            Office = application.Placement?.Office is not null ? new OfficeInfo
            {
                Id = application.Placement.Office.Id,
                OfficeName = application.Placement.Office.OfficeName,
                IsDeleted = application.Placement.Office.IsDeleted,
                CreatedAt = application.Placement.Office.CreatedAt,
                UpdatedAt = application.Placement.Office.UpdatedAt,
                DeletedAt = application.Placement.Office.DeletedAt
            } : null
        };
    }

    public async Task RejectApplication(Guid uuid, CancellationToken ct)
    {
        var application = await _context.Students
        .Include(t => t.Application)
        .Include(t => t.Requirements)
        .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, ct)
        ?? throw new KeyNotFoundException("Application not found");


        if(application.Application.Status == Shared.Enums.ApplicationStatusEnum.Approved)
        {
            throw new Exception("Approved application cannot be rejected");
        }

        application.Application.Status = Shared.Enums.ApplicationStatusEnum.Rejected;
        _context.SaveChanges();

    }

    public async Task Trash(Guid uuid, CancellationToken ct)
    {
        var application = await _context.Students
        .Include(t => t.Requirements)
        .Include(t => t.Application)
        .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, cancellationToken: ct)
        ?? throw new KeyNotFoundException("Application not found");

        application.Application.IsDeleted = true;
        application.Application.DeletedAt = DateTime.Now;

        application.IsDeleted = true;
        application.DeletedAt = DateTime.Now;

        application.Requirements.Select(t =>
        {
            t.IsDeleted = true;
            t.DeletedAt = DateTime.Now;
            return t;
        });
        await _context.SaveChangesAsync(ct);
    }
}
