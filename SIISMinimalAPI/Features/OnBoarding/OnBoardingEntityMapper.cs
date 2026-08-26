using System;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.OnBoarding;

public static class OnBoardingEntityMapper
{
    public static Student ToStudent(OnBoardingDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto.Student);
        ArgumentNullException.ThrowIfNull(dto.School);
        ArgumentNullException.ThrowIfNull(dto.Internship);

        var student = dto.Student;
        var school = dto.School;
        var internship = dto.Internship;

        return new Student
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
            SchoolName = school.Name,
            SchoolAddress = school.Address,
            SchoolContactPerson = school.ContactPerson,
            SchoolContactPersonEmail = school.Email,
            SchoolContactPersonPhone = school.ContactNumber,
            InternshipNature = internship.InternshipNature,
            Strand = internship.Strand ?? StrandEnum.STEM,
            Degree = internship.Degree ?? DegreeEnum.BSIT,
            TotalInternshipHours = internship.InternshipTotalHours,
            Application = new Shared.Models.Application
            {
                Uuid = Guid.NewGuid(),
                Status = ApplicationStatusEnum.Pending,
            },
            Requirements = dto.RequirementsReg.Select(t => new Requirement
            {
                FileName = t.FileName,
                FilePath = t.FilePath,
                FileType = t.FileType,
                RequirementType = t.RequirementTypeEnum
            }).ToList(),
        };
    }
}