
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Logs;
using SIISMinimalAPI.Features.Application.AssignAndApprove;
using SIISMinimalAPI.Features.Application.GetById;
using SIISMinimalAPI.Features.Shared.Models;
using Humanizer;
using SIISMinimalAPI.Features.Email;

namespace SIISMinimalAPI.Features.Application;

public class ApplicationHandler(AppDbContext context, ILogService logService, IEmailService emailService, ILogger<ApplicationHandler> logger) : IApplicationService
{
    private readonly AppDbContext _context = context;
    private readonly ILogService _logService = logService;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<ApplicationHandler> _logger = logger;

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
                AccumulatedHours = 0,
                Progresses = new List<Shared.Models.Progress>
                {
                    new Shared.Models.Progress
                    {
                        TrainingHoursRendered = 0,
                        TrainingHoursForWeek = 0,
                        RemainingHours = exists.TotalInternshipHours,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                }
            };
        }
        else
        {
            exists.Placement.OfficeId = office.Id;
        }

        exists.Application.Status = Shared.Enums.ApplicationStatusEnum.Approved;
        exists.Application.UpdatedAt = DateTime.Now;



        string subject = "Update on Your Internship Application";
        string htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #f9f9f9; }}
                        .header {{ background-color: #102e6c; color: white; padding: 10px 20px; border-radius: 6px 6px 0 0; text-align: center; }}
                        .content {{ padding: 20px; background-color: white; }}
                        .footer {{ font-size: 12px; color: #777; text-align: center; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Application Status Update</h2>
                        </div>
                        <div class='content'>
                            <p>Hi <strong>{exists.FullName}</strong>,</p>
                         <p>We are excited to inform you that your internship application has been <strong>approved</strong>!</p>
                        <div class='details'>
                            <p style='margin: 5px 0;'><strong>Assigned Office:</strong> {office.OfficeName}</p>
                            <p style='margin: 5px 0;'><strong>Start Date:</strong> {exists.Placement.StartDate:yyyy-MM-dd}</p>
                            <p style='margin: 5px 0;'><strong>Estimated End Date:</strong> {exists.Placement.EstimatedEndDate:yyyy-MM-dd}</p>
                        </div>

                        <p>Please log in to your student portal for further instructions regarding your placement guidelines and first-day orientation.</p>
                        <p>Welcome aboard!</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} SIIS. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

        await _context.SaveChangesAsync(ct);
        try
        {
            await _emailService.SendEmailAsync(exists.Email, "Internship Application", htmlBody);
        }
        catch (Exception ex)
        {
            // Log the email error, but let the approval process succeed
            _logger.LogError(ex, "Failed to send approval email to {Email}", exists.Email);
        }
        var userId = context.Entry(exists).Property("Id").CurrentValue.ToString() ?? "unknown";
        await _logService.WriteAsync("Approve", "Application", exists.Application.Id, userId, $"Approved application for {exists.FullName}");
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

        var deleteUserId = context.Entry(application).Property("Id").CurrentValue.ToString() ?? "unknown";
        await _logService.WriteAsync("Delete", "Application", application.Application.Id, deleteUserId, $"Deleted application for {application.FullName}");
    }

    public async Task<ICollection<ApplicationDto>> GetAllAsync(CancellationToken ct)
    {
        var applications = await _context.Students
        .Include(t => t.Application)
        .Include(t => t.Placement).ThenInclude(p => p.Office)
        .AsSplitQuery()
        .AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken: ct);

        return [.. applications.Select(t => {

           var degreeStrand = t.Degree?.ToString().Humanize(LetterCasing.Title)
                ?? t.Strand?.ToString().Humanize(LetterCasing.Title)
                ?? "N/A";

             return new ApplicationDto
         {
             Id = t.Application.Id,
             ApplicationUUID = t.Application.ApplicationUUID,
             StudentUUID = t.StudentUUID,
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

        if (application is null) throw new KeyNotFoundException("Application not found");

        return MapToDto(application);
    }

    public async Task<ApplicationGetByIdDto> GetByStudentUuidAsync(Guid studentUuid, CancellationToken ct)
    {
        var application = await _context.Students
         .Include(t => t.Requirements)
         .Include(t => t.Application)
         .Include(t => t.Placement).ThenInclude(p => p.Office)
         .AsSplitQuery()
         .AsNoTracking()
          .FirstOrDefaultAsync(t => t.StudentUUID == studentUuid, cancellationToken: ct);

        if (application is null) throw new KeyNotFoundException("Application not found");

        return MapToDto(application);
    }

    private static ApplicationGetByIdDto MapToDto(Student application)
    {
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
             RequirementType = t.RequirementType,
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

    public async Task RejectApplication(Guid uuid, string? reason, CancellationToken ct)
    {
        var application = await _context.Students
        .Include(t => t.Application)
        .Include(t => t.Requirements)
        .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, ct)
        ?? throw new KeyNotFoundException("Application not found");


        if (application.Application.Status == Shared.Enums.ApplicationStatusEnum.Approved)
        {
            throw new Exception("Approved application cannot be rejected");
        }

        application.Application.Status = Shared.Enums.ApplicationStatusEnum.Rejected;
        application.Application.Reason = reason;
        await _context.SaveChangesAsync(ct);

        var rejectUserId = context.Entry(application).Property("Id").CurrentValue.ToString() ?? "unknown";
        await _logService.WriteAsync("Reject", "Application", application.Application.Id, rejectUserId, $"Rejected application for {application.FullName}: {reason}");

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

        var trashUserId = context.Entry(application).Property("Id").CurrentValue.ToString() ?? "unknown";
        await _logService.WriteAsync("Trash", "Application", application.Application.Id, trashUserId, $"Trashed application for {application.FullName}");
    }
}
