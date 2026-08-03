
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
     .Include(t => t.Office)
     .Include(t => t.Application)
     .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, ct)
     ?? throw new KeyNotFoundException("Application not found");

        var office = await _context.Offices
            .FirstOrDefaultAsync(t => t.Name == requestDto.Office, ct)
            ?? throw new KeyNotFoundException("No office found");


        exists.OfficeId = office.Id;

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
        .Include(t => t.Application).Include(t => t.Internship).AsSplitQuery()
        .AsNoTracking().OrderByDescending(t => t.CreateAt).ToListAsync(cancellationToken: ct);

        return [.. applications.Select(t => {

           var degreeStrand = t.Internship.Degree?.ToString()
                ?? t.Internship.Strand?.ToString();

            return new ApplicationDto
        {
            Id = t.Application.Id,
            ApplicationUUID = t.Application.ApplicationUUID,
            FullName = $"{t.LastName}, {t.FirstName} {t.MiddleName}".Trim(),
            Status = t.Application.Status.ToString(),
            DegreeStrand = degreeStrand,
            CreatedAt = t.Application.CreateAt,
            UpdatedAt = t.Application.UpdatedAt
        };
        })];
    }

    public async Task<ApplicationGetByIdDto> GetByIdAsync(Guid uuid, CancellationToken ct)
    {
        var application = await _context.Students
         .Include(t => t.School)
         .Include(t => t.Internship)
         .Include(t => t.Requirements)
         .Include(t => t.Application)
         .Include(t => t.Office)
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
                IsDeleted = application.IsDeleted,
                CreateAt = application.CreateAt,
                UpdatedAt = application.UpdatedAt,
                DeletedAt = application.DeletedAt,
                OfficeId = application.OfficeId
            },

            Application = new ApplicationInfo
            {
                Id = application.Application.Id,
                ApplicationUUID = application.Application.ApplicationUUID,
                Status = application.Application.Status,
                IsDeleted = application.Application.IsDeleted,
                CreateAt = application.Application.CreateAt,
                UpdatedAt = application.Application.UpdatedAt,
                DeletedAt = application.Application.DeletedAt
            },
            School = application.School is null ? null : new SchoolInfo
            {
                Id = application.School.Id,
                Name = application.School.Name,
                Address = application.School.Address,
                ContactPerson = application.School.ContactPerson,      // Fixed
                Email = application.School.Email,
                ContactNumber = application.School.ContactNumber,
                IsDeleted = application.School.IsDeleted,
                CreateAt = application.School.CreateAt,
                UpdatedAt = application.School.UpdatedAt,
                DeletedAt = application.School.DeletedAt
            },
            Internship = application.Internship is null ? null : new InternshipInfo
            {
                Id = application.Internship.Id,
                InternshipNature = application.Internship.InternshipNature,
                Strand = application.Internship.Strand,
                Degree = application.Internship.Degree,
                StartDate = application.Internship.StartDate,
                EstimatedEndDate = application.Internship.EstimatedEndDate,
                InternshipTotalHours = application.Internship.InternshipTotalHours,
                IsDeleted = application.Internship.IsDeleted,
                CreateAt = application.Internship.CreateAt,
                UpdatedAt = application.Internship.UpdatedAt,
                DeletedAt = application.Internship.DeletedAt
            },
            Requirements = application.Requirements?
         .Where(r => !r.IsDeleted)  // Optional: exclude soft-deleted
         .Select(t => new RequirementInfo
         {
             Id = t.Id,
             FileName = t.FileName,
             FilePath = t.FilePath,
             FileType = t.FileType,
             IsDeleted = t.IsDeleted,
             CreateAt = t.CreateAt,
             UpdatedAt = t.UpdatedAt,
             DeletedAt = t.DeletedAt
         }).ToList(),
            Office = application.Office is null ? null : new OfficeInfo
            {
                Id = application.Office.Id,
                Name = application.Office.Name,
                Department = application.Office.Department,
                IsDeleted = application.Office.IsDeleted,
                CreateAt = application.Office.CreateAt,
                UpdatedAt = application.Office.UpdatedAt,
                DeletedAt = application.Office.DeletedAt
            }
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
        .Include(t => t.School)
        .Include(t => t.Internship)
        .Include(t => t.Requirements)
        .Include(t => t.Application)
        .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, cancellationToken: ct)
        ?? throw new KeyNotFoundException("Application not found");

        application.School.IsDeleted = true;
        application.School.DeletedAt = DateTime.Now;

        application.Application.IsDeleted = true;
        application.Application.DeletedAt = DateTime.Now;

        application.IsDeleted = true;
        application.DeletedAt = DateTime.Now;

        application.Internship.IsDeleted = true;
        application.Internship.DeletedAt = DateTime.Now;
        application.Requirements.Select(t =>
        {
            t.IsDeleted = true;
            t.DeletedAt = DateTime.Now;
            return t;
        });
        await _context.SaveChangesAsync(ct);
    }
}
