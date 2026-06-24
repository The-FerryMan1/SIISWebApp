using System.Data;
using System.Data.Common;
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
                var existingStud = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.Email.ToLower() == onBoardingDto.Student.Email.ToLower(),
                    ct);
                if (existingStud is not null)
                {
                    throw new DuplicateNameException("Student with this email is already registered");
                }

                var newOnboadingUser = OnBoardingEntityMapper.ToStudentModel(onBoardingDto);
                await _context.AddAsync(newOnboadingUser);
                await _context.SaveChangesAsync();
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

            var existsOffice = await _context.Offices.FirstOrDefaultAsync(t => t.Name == dto.Office.Name)
            ?? throw new KeyNotFoundException("Application not found");
            // 1. Update Student (root entity) - map DTO directly, not through mapper
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

            // 2. Update School

            exists.School.Name = dto.School.Name;
            exists.School.Address = dto.School.Address;
            exists.School.ContactPerson = dto.School.ContactPerson;
            exists.School.Email = dto.School.Email;
            exists.School.ContactNumber = dto.School.ContactNumber;


            // 3. Update Internship

            exists.Internship.InternshipNature = dto.Internship.InternshipNature;
            exists.Internship.Strand = dto.Internship.Strand;
            exists.Internship.Degree = dto.Internship.Degree;
            exists.Internship.StartDate = dto.Internship.StartDate;
            exists.Internship.EstimatedEndDate = dto.Internship.EstimatedEndDate;
            exists.Internship.InternshipTotalHours = dto.Internship.InternshipTotalHours;
            exists.Internship.UpdatedAt = DateTime.UtcNow;
            exists.Office.Id = existsOffice.Id;
            await _context.SaveChangesAsync(ct);
        }
    }
}
