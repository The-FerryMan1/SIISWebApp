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
using SIISMinimalAPI.Features.Notifications;

namespace SIISMinimalAPI.Features.Application;

public class ApplicationHandler(AppDbContext context, ILogService logService, IEmailService emailService, ILogger<ApplicationHandler> logger, INotificationService notificationService) : IApplicationService
{
    private readonly AppDbContext _context = context;
    private readonly ILogService _logService = logService;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<ApplicationHandler> _logger = logger;
    private readonly INotificationService _notificationService = notificationService;

    private const string BaseUrl = "https://siis.example.com";
    private const string LogoUrl = BaseUrl + "/uploads/system/logo.png";

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

        await _context.SaveChangesAsync(ct);

        await _notificationService.NotifyOfficeAsync(
            office.Id,
            "AdminApplicationApproved",
            "Application Approved",
            $"{exists.FullName}'s application was approved and assigned to your office.",
            "/office-dashboard",
            "High",
            ct);

        // Fire-and-forget email to avoid blocking the response
        _ = Task.Run(async () =>
        {
            try
            {
                var subject = "✅ Your Internship Application Has Been Approved!";
                var htmlBody = BuildApprovalEmail(exists.FullName, office.OfficeName, exists.Placement.StartDate, exists.Placement.EstimatedEndDate, exists.TotalInternshipHours);
                await _emailService.SendEmailAsync(exists.Email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval email to {Email}", exists.Email);
            }
        });

        var userId = context.Entry(exists).Property("Id").CurrentValue.ToString() ?? "unknown";
        await _logService.WriteAsync("Approve", "Application", exists.Application.Id, userId, $"Approved application for {exists.FullName}");
    }

    private static string BuildApprovalEmail(string studentName, string officeName, DateOnly startDate, DateOnly endDate, int totalHours)
    {
        var year = DateTime.Now.Year;
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Application Approved</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif; background-color: #f3f4f6;'>
    <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07); overflow: hidden;'>
        <tr>
            <td style='background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%); padding: 40px 30px; text-align: center;'>
                <img src='{LogoUrl}' alt='SIIS Logo' style='width: 80px; height: 80px; object-fit: contain; background: white; border-radius: 12px; padding: 8px; margin-bottom: 16px;' onerror='this.style.display=""none""' />
                <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: 700; letter-spacing: -0.5px;'>Application Approved</h1>
                <p style='margin: 8px 0 0; color: rgba(255, 255, 255, 0.9); font-size: 16px;'>Student Internship Information System</p>
            </td>
        </tr>
        <tr>
            <td style='padding: 30px; text-align: center;'>
                <div style='display: inline-block; background: #dcfce7; color: #166534; padding: 12px 28px; border-radius: 9999px; font-weight: 600; font-size: 16px;'>
                    ✅ Status: Approved
                </div>
            </td>
        </tr>
        <tr>
            <td style='padding: 0 30px 30px;'>
                <p style='font-size: 18px; color: #1f2937; margin: 0 0 8px;'>Hi <strong>{studentName}</strong>,</p>
                <p style='font-size: 16px; color: #4b5563; line-height: 1.7; margin: 0 0 24px;'>Congratulations! We're excited to inform you that your internship application has been <strong style='color: #166534;'>approved</strong>. Welcome to the program!</p>
                <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='border: 1px solid #e5e7eb; border-radius: 10px; overflow: hidden; margin-bottom: 24px;'>
                    <tr>
                        <td style='background: #f9fafb; padding: 16px 20px; border-bottom: 1px solid #e5e7eb;'>
                            <h2 style='margin: 0; font-size: 18px; font-weight: 600; color: #1f2937;'>📋 Placement Details</h2>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 20px;'>
                            <table role='presentation' width='100%' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td style='padding: 12px 0; border-bottom: 1px solid #f3f4f6; width: 40%;'>
                                        <span style='font-size: 14px; color: #6b7280; font-weight: 500;'>Assigned Office</span>
                                    </td>
                                    <td style='padding: 12px 0; border-bottom: 1px solid #f3f4f6;'>
                                        <span style='font-size: 16px; font-weight: 600; color: #1f2937;'>{officeName}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 0; border-bottom: 1px solid #f3f4f6;'>
                                        <span style='font-size: 14px; color: #6b7280; font-weight: 500;'>Start Date</span>
                                    </td>
                                    <td style='padding: 12px 0; border-bottom: 1px solid #f3f4f6;'>
                                        <span style='font-size: 16px; font-weight: 600; color: #1f2937;'>{startDate:MMMM dd, yyyy}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 0; border-bottom: 1px solid #f3f4f6;'>
                                        <span style='font-size: 14px; color: #6b7280; font-weight: 500;'>Estimated End Date</span>
                                    </td>
                                    <td style='padding: 12px 0; border-bottom: 1px solid #f3f4f6;'>
                                        <span style='font-size: 16px; font-weight: 600; color: #1f2937;'>{endDate:MMMM dd, yyyy}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 0;'>
                                        <span style='font-size: 14px; color: #6b7280; font-weight: 500;'>Total Internship Hours</span>
                                    </td>
                                    <td style='padding: 12px 0;'>
                                        <span style='font-size: 16px; font-weight: 600; color: #1f2937;'>{totalHours} hours</span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
                <div style='background: #eff6ff; border: 1px solid #bfdbfe; border-radius: 10px; padding: 20px; margin-bottom: 24px;'>
                    <h3 style='margin: 0 0 12px; font-size: 16px; font-weight: 600; color: #1e40af; display: flex; align-items: center; gap: 8px;'>
                        <span>🚀</span> Next Steps
                    </h3>
                    <ul style='margin: 0; padding-left: 20px; color: #1e40af; font-size: 14px; line-height: 1.8;'>
                        <li>Log in to your <strong>student portal</strong> for placement guidelines</li>
                        <li>Review first-day orientation details and requirements</li>
                        <li>Coordinate with your assigned office supervisor</li>
                        <li>Prepare necessary documents for onboarding</li>
                    </ul>
                </div>
                <div style='text-align: center; margin-bottom: 24px;'>
                    <a href='{BaseUrl}/student/portal' style='display: inline-block; background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%); color: #ffffff; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 16px; box-shadow: 0 4px 14px rgba(30, 58, 138, 0.4);'>
                        Access Student Portal →
                    </a>
                </div>
            </td>
        </tr>
        <tr>
            <td style='background: #f9fafb; padding: 24px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                <p style='margin: 0 0 8px; font-size: 13px; color: #9ca3af;'>
                    This is an automated notification from the Student Internship Information System (SIIS).
                </p>
                <p style='margin: 0; font-size: 12px; color: #9ca3af;'>
                    © {year} SIIS. All rights reserved.
                </p>
                <p style='margin: 12px 0 0; font-size: 12px; color: #9ca3af;'>
                    Need help? Contact support at support@siis.example.com
                </p>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string BuildRejectionEmail(string studentName, string reason)
    {
        var year = DateTime.Now.Year;
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Application Rejected</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif; background-color: #f3f4f6;'>
    <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07); overflow: hidden;'>
        <tr>
            <td style='background: linear-gradient(135deg, #991b1b 0%, #ef4444 100%); padding: 40px 30px; text-align: center;'>
                <img src='{LogoUrl}' alt='SIIS Logo' style='width: 80px; height: 80px; object-fit: contain; background: white; border-radius: 12px; padding: 8px; margin-bottom: 16px;' onerror='this.style.display=""none""' />
                <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: 700; letter-spacing: -0.5px;'>Application Update</h1>
                <p style='margin: 8px 0 0; color: rgba(255, 255, 255, 0.9); font-size: 16px;'>Student Internship Information System</p>
            </td>
        </tr>
        <tr>
            <td style='padding: 30px; text-align: center;'>
                <div style='display: inline-block; background: #fef2f2; color: #991b1b; padding: 12px 28px; border-radius: 9999px; font-weight: 600; font-size: 16px; border: 1px solid #fecaca;'>
                    ❌ Status: Not Approved
                </div>
            </td>
        </tr>
        <tr>
            <td style='padding: 0 30px 30px;'>
                <p style='font-size: 18px; color: #1f2937; margin: 0 0 8px;'>Hi <strong>{studentName}</strong>,</p>
                <p style='font-size: 16px; color: #4b5563; line-height: 1.7; margin: 0 0 24px;'>Thank you for your interest in our internship program. After careful review, we regret to inform you that your application has not been approved at this time.</p>
                
                <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='border: 1px solid #e5e7eb; border-radius: 10px; overflow: hidden; margin-bottom: 24px;'>
                    <tr>
                        <td style='background: #fef2f2; padding: 16px 20px; border-bottom: 1px solid #fecaca;'>
                            <h2 style='margin: 0; font-size: 18px; font-weight: 600; color: #991b1b;'>📋 Decision Details</h2>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 20px;'>
                            <table role='presentation' width='100%' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td style='padding: 12px 0; border-bottom: 1px solid #f3f4f6; width: 40%;'>
                                        <span style='font-size: 14px; color: #6b7280; font-weight: 500;'>Reason</span>
                                    </td>
                                    <td style='padding: 12px 0; border-bottom: 1px solid #f3f4f6;'>
                                        <span style='font-size: 16px; font-weight: 500; color: #991b1b;'>{reason}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 0;'>
                                        <span style='font-size: 14px; color: #6b7280; font-weight: 500;'>Status</span>
                                    </td>
                                    <td style='padding: 12px 0;'>
                                        <span style='font-size: 16px; font-weight: 600; color: #991b1b;'>Rejected</span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
                
                <div style='background: #fef3c7; border: 1px solid #fcd34d; border-radius: 10px; padding: 20px; margin-bottom: 24px;'>
                    <h3 style='margin: 0 0 12px; font-size: 16px; font-weight: 600; color: #92400e; display: flex; align-items: center; gap: 8px;'>
                        <span>💡</span> What You Can Do Next
                    </h3>
                    <ul style='margin: 0; padding-left: 20px; color: #92400e; font-size: 14px; line-height: 1.8;'>
                        <li>Review the reason provided and address any gaps in your application</li>
                        <li>Consider applying for future internship cycles</li>
                        <li>Reach out to your school's placement coordinator for guidance</li>
                        <li>You may reapply once requirements are met</li>
                    </ul>
                </div>
                
                <div style='text-align: center; margin-bottom: 24px;'>
                    <a href='{BaseUrl}/student/portal' style='display: inline-block; background: #f3f4f6; color: #374151; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 16px; border: 1px solid #d1d5db;'>
                        View Application Details
                    </a>
                </div>
            </td>
        </tr>
        <tr>
            <td style='background: #f9fafb; padding: 24px 30px; border-top: 1px solid #e5e7eb; text-align: center;'>
                <p style='margin: 0 0 8px; font-size: 13px; color: #9ca3af;'>
                    This is an automated notification from the Student Internship Information System (SIIS).
                </p>
                <p style='margin: 0; font-size: 12px; color: #9ca3af;'>
                    © {year} SIIS. All rights reserved.
                </p>
                <p style='margin: 12px 0 0; font-size: 12px; color: #9ca3af;'>
                    Need help? Contact support at support@siis.example.com
                </p>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct)
    {
        var application = await _context.Students
        .Include(t => t.Application)
        .Include(t => t.Requirements)
        .Include(t => t.Placement)
        .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, ct)
        ?? throw new KeyNotFoundException("Application not found");

        var officeId = application.Placement?.OfficeId;

        foreach (var req in application.Requirements)
        {
            if (!string.IsNullOrEmpty(req.FilePath) && File.Exists(req.FilePath))
            {
                File.Delete(req.FilePath);
            }
        }

        _context.Remove(application);
        await _context.SaveChangesAsync(ct);
        if (officeId.HasValue)
        {
            await _notificationService.NotifyOfficeAsync(
                officeId.Value,
                "AdminApplicationDeleted",
                "Application Deleted",
                $"{application.FullName}'s application was deleted by the administrator.",
                "/office-dashboard",
                "High",
                ct);
        }

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
        .Include(t => t.Placement)
        .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, ct)
        ?? throw new KeyNotFoundException("Application not found");


        if (application.Application.Status == Shared.Enums.ApplicationStatusEnum.Approved)
        {
            throw new Exception("Approved application cannot be rejected");
        }

        application.Application.Status = Shared.Enums.ApplicationStatusEnum.Rejected;
        application.Application.Reason = reason;
        await _context.SaveChangesAsync(ct);

        await _notificationService.NotifyOfficeForStudentAsync(
            application.Id,
            "AdminApplicationRejected",
            "Application Rejected",
            $"{application.FullName}'s application was rejected by the administrator.",
            "/office-dashboard",
            "Medium",
            ct);

        // Send rejection email (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var subject = "❌ Update on Your Internship Application";
                var rejectionReason = string.IsNullOrWhiteSpace(reason) ? "Not specified" : reason;
                var htmlBody = BuildRejectionEmail(application.FullName, rejectionReason);
                await _emailService.SendEmailAsync(application.Email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rejection email to {Email}", application.Email);
            }
        });

        var rejectUserId = context.Entry(application).Property("Id").CurrentValue.ToString() ?? "unknown";
        await _logService.WriteAsync("Reject", "Application", application.Application.Id, rejectUserId, $"Rejected application for {application.FullName}: {reason}");

    }

    public async Task Trash(Guid uuid, CancellationToken ct)
    {
        var application = await _context.Students
        .Include(t => t.Requirements)
        .Include(t => t.Application)
        .Include(t => t.Placement)
        .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, cancellationToken: ct)
        ?? throw new KeyNotFoundException("Application not found");

        var officeId = application.Placement?.OfficeId;

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
        if (officeId.HasValue)
        {
            await _notificationService.NotifyOfficeAsync(
                officeId.Value,
                "AdminApplicationTrashed",
                "Application Removed",
                $"{application.FullName}'s application was removed by the administrator.",
                "/office-dashboard",
                "Medium",
                ct);
        }

        var trashUserId = context.Entry(application).Property("Id").CurrentValue.ToString() ?? "unknown";
        await _logService.WriteAsync("Trash", "Application", application.Application.Id, trashUserId, $"Trashed application for {application.FullName}");
    }
}