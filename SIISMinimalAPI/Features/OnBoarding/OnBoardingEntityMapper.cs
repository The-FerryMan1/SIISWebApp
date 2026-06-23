using System;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.OnBoarding;

public static class OnBoardingEntityMapper
{
    public static StudentModel ToStudentModel(OnBoardingDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto.Student);
        ArgumentNullException.ThrowIfNull(dto.School);
        ArgumentNullException.ThrowIfNull(dto.Internship);
        ArgumentNullException.ThrowIfNull(dto.Requirements);

        var student = dto.Student;
        var school = dto.School;
        var internship = dto.Internship;
        var requirements = dto.Requirements;
        return new StudentModel
        {
            Email = student.Email,
            LastName = student.LastName,
            FirstName = student.FirstName,
            MiddleName = student.MiddleName,
            Address = student.Address,
            ContactNumber = student.ContactNumber,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            GradeLevel = student.GradeLevel,
            School = new SchoolModel
            {
                Name = school.Name,
                Address = school.Address,
                ContactNumber = school.ContactNumber,
                ContactPerson = school.ContactPerson,
                Email = school.ContactPerson
            },
             Internship = new InternshipModel
             {
                 InternshipNature = internship.InternshipNature,
                 Degree = internship.Degree,
                 Strand = internship.Strand,
                 EstimatedEndDate = internship.EstimatedEndDate,
                 InternshipTotalHours = internship.InternshipTotalHours,
                 StartDate = internship.StartDate,
             },
            Requirements = dto.Requirements.Select(t => new RequirementModel
            {
                FileName = t.FileName,
                FilePath = t.FilePath,
                FileType = t.FileType
            }).ToList(),
             Application = new ApplicationModel
             {
                 ApplicationUUID = Guid.NewGuid(),
             }
        };
    }
}
