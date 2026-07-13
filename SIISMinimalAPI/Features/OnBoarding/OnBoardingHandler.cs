using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;

namespace SIISMinimalAPI.Features.OnBoarding
{
    public class OnBoardingHandler(AppDbContext context) : IOnBoadringService
    {
        private readonly AppDbContext _context = context;
        public async Task CreateOnBoarding(OnBoardingDto onBoardingDto, CancellationToken ct)
        {
            try
            {
                if (onBoardingDto.Student is null)
                {
                    throw new ArgumentException("Student information is required");
                }

                var existingStud = await _context.Students.AsNoTracking()
                    .FirstOrDefaultAsync(
                        t => t.Email.ToLower() == onBoardingDto.Student.Email.ToLower(),
                        ct);
                if (existingStud is not null)
                {
                    throw new DuplicateNameException("Student with this email is already registered");
                }

                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", onBoardingDto.Student.LastName);
                Directory.CreateDirectory(uploadsPath);

                var req = new List<RequirementsRegDto>();
                if (onBoardingDto.Files is not null)
                {
                    foreach (var file in onBoardingDto.Files)
                    {
                        var filePath = Path.Combine(uploadsPath, file.FileName);
                        await using var stream = File.Create(filePath);
                        await file.CopyToAsync(stream, ct);

                        req.Add(new RequirementsRegDto
                        {
                            FileName = file.FileName,
                            FilePath = filePath,
                            FileType = file.ContentType
                        });
                    }
                }

                onBoardingDto.RequirementsReg = req;
                var newOnboadingUser = OnBoardingEntityMapper.ToStudentModel(onBoardingDto);
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
                .Include(t => t.School)
                .Include(t => t.Internship)
                .Include(t => t.Office)
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
            if (dto.School is not null && exists.School is not null)
            {
                exists.School.Name = dto.School.Name;
                exists.School.Address = dto.School.Address;
                exists.School.ContactPerson = dto.School.ContactPerson;
                exists.School.Email = dto.School.Email;
                exists.School.ContactNumber = dto.School.ContactNumber;
                exists.School.UpdatedAt = DateTime.UtcNow;
            }

            // Internship
            if (dto.Internship is not null && exists.Internship is not null)
            {
                exists.Internship.InternshipNature = dto.Internship.InternshipNature;
                exists.Internship.Strand = dto.Internship.Strand;
                exists.Internship.Degree = dto.Internship.Degree;
                exists.Internship.StartDate = dto.Internship.StartDate;
                exists.Internship.EstimatedEndDate = dto.Internship.EstimatedEndDate;
                exists.Internship.InternshipTotalHours = dto.Internship.InternshipTotalHours;
                exists.Internship.UpdatedAt = DateTime.UtcNow;
            }

            // Office (optional reassignment)
            if (dto.Office is not null && exists.Office is not null)
            {
                exists.Office.Name = dto.Office.Name;
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
                    _context.Requirements.Remove(requirement); // adjust DbSet name if different
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
                        exists.Requirements.Add(new Shared.Models.RequirementModel // adjust entity type name
                        {
                            FileName = reqDto.FileName,
                            FilePath = reqDto.FilePath,
                            FileType = reqDto.FileType,
                        });
                    }
                }
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}
