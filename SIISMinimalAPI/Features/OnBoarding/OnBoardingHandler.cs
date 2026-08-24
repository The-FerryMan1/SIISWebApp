using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Logs;

namespace SIISMinimalAPI.Features.OnBoarding
{
    public class OnBoardingHandler(AppDbContext context, ILogService logService) : IOnBoadringService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogService _logService = logService;
    public async Task CreateOnBoarding(OnBoardingDto onBoardingDto, CancellationToken ct)
    {
        try
        {
            if (onBoardingDto.Student is null)
            {
                throw new ArgumentException("Student information is required");
            }

            if (string.IsNullOrWhiteSpace(onBoardingDto.Student.Email))
            {
                throw new ArgumentException("Student email is required");
            }

            var existingStud = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.Email.ToLower() == onBoardingDto.Student.Email.ToLower(),
                    ct);
            if (existingStud is not null)
            {
                throw new DuplicateNameException("Student with this email is already registered");
            }

                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", SanitizeFolderName(onBoardingDto.Student.LastName));
                Directory.CreateDirectory(uploadsPath);

                var req = new List<RequirementsRegDto>();
                if (onBoardingDto.Files is not null)
                {
                    foreach (var file in onBoardingDto.Files)
                    {
                        var safeFileName = Path.GetFileNameWithoutExtension(file.FileName) + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsPath, safeFileName);
                        await using var stream = File.Create(filePath);
                        await file.CopyToAsync(stream, ct);

                        req.Add(new RequirementsRegDto
                        {
                            FileName = safeFileName,
                            FilePath = filePath,
                            FileType = file.ContentType
                        });
                    }
                }

                onBoardingDto.RequirementsReg = req;
                var newOnboadingUser = OnBoardingEntityMapper.ToStudent(onBoardingDto);
                await _context.AddAsync(newOnboadingUser, ct);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdatedOnBoarding(Guid uuid, OnBoardUpdateDto dto, CancellationToken ct)
        {
            var exists = await _context.Students
                .Include(t => t.Application)
                .Include(t => t.Placement)
                .Include(t => t.Requirements)
                .AsSplitQuery()
                .FirstOrDefaultAsync(t => t.Application.ApplicationUUID == uuid, ct)
                ?? throw new KeyNotFoundException("Application not found");

            // Student
            if (dto.Student is not null)
            {
                exists.FirstName = dto.Student.FirstName;
                exists.LastName = dto.Student.LastName;
                exists.MiddleName = dto.Student.MiddleName;
                exists.Email = dto.Student.Email;
                exists.ContactNumber = dto.Student.ContactNumber;
                exists.Address = dto.Student.Address;
                exists.DateOfBirth = dto.Student.DateOfBirth;
                exists.Gender = dto.Student.Gender;
                exists.GradeLevel = dto.Student.GradeLevel;
                exists.UpdatedAt = DateTime.UtcNow;
            }

            // School
            if (dto.School is not null)
            {
                exists.SchoolName = dto.School.Name;
                exists.SchoolAddress = dto.School.Address;
                exists.SchoolContactPerson = dto.School.ContactPerson;
                exists.SchoolContactPersonEmail = dto.School.Email;
                exists.SchoolContactPersonPhone = dto.School.ContactNumber;
            }

            // Internship
            if (dto.Internship is not null)
            {
                exists.InternshipNature = dto.Internship.InternshipNature;
                exists.Strand = dto.Internship.Strand ?? StrandEnum.STEM;
                exists.Degree = dto.Internship.Degree ?? DegreeEnum.BSIT;
                exists.TotalInternshipHours = dto.Internship.InternshipTotalHours;
            }

            // Placement (optional reassignment)
            if (dto.Office is not null && exists.Placement is not null)
            {
                var office = await _context.Offices
                    .FirstOrDefaultAsync(t => t.OfficeName == dto.Office.Name, ct)
                    ?? throw new KeyNotFoundException("No office found");

                exists.Placement.OfficeId = office.Id;
            }

            // Requirements sync (add / update / remove by FilePath)
            if (dto.Requirements is not null)
            {
                var incomingPaths = dto.Requirements
                    .Where(r => r.FilePath is not null)
                    .Select(r => r.FilePath)
                    .ToHashSet();

                var toRemove = exists.Requirements
                    .Where(r => !incomingPaths.Contains(r.FilePath))
                    .ToList();

                foreach (var requirement in toRemove)
                {
                    exists.Requirements.Remove(requirement);
                    _context.Requirements.Remove(requirement);
                }

                foreach (var reqDto in dto.Requirements)
                {
                    var match = exists.Requirements
                        .FirstOrDefault(r => r.FilePath == reqDto.FilePath);

                    if (match is not null)
                    {
                        match.FileName = reqDto.FileName;
                        match.FileType = reqDto.FileType;
                    }
                    else
                    {
                        exists.Requirements.Add(new Shared.Models.Requirement
                        {
                            FileName = reqDto.FileName,
                            FilePath = reqDto.FilePath,
                            FileType = reqDto.FileType,
                        });
                    }
                }
            }

            // Save new uploaded files
            if (dto.Files is not null && dto.Files.Count > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", SanitizeFolderName(exists.LastName));
                Directory.CreateDirectory(uploadsPath);

                foreach (var file in dto.Files)
                {
                    var safeFileName = Path.GetFileNameWithoutExtension(file.FileName) + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsPath, safeFileName);
                    await using var stream = File.Create(filePath);
                    await file.CopyToAsync(stream, ct);

                    exists.Requirements.Add(new Shared.Models.Requirement
                    {
                        FileName = safeFileName,
                        FilePath = filePath,
                        FileType = file.ContentType,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync(ct);

            var updateUserId = context.Entry(exists).Property("Id").CurrentValue?.ToString() ?? "unknown";
            await _logService.WriteAsync("Update", "OnBoarding", exists.Id, updateUserId, $"Updated on-boarding for {exists.FullName}");
        }

        private static string SanitizeFolderName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
            return string.IsNullOrWhiteSpace(sanitized) ? "Unknown" : sanitized;
        }
    }
}
