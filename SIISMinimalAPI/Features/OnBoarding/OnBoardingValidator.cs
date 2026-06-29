using System;
using FluentValidation;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.OnBoarding;

public class OnBoardingDtoValidator : AbstractValidator<OnBoardingDto>
{
    public OnBoardingDtoValidator(
        IValidator<StudentRegDto> studentValidator,
        IValidator<SchoolRegDto> schoolValidator,
        IValidator<InternshipRegDto> internshipValidator,
        IValidator<RequirementsRegDto> requirementValidator)
    {
        RuleFor(x => x.Student)
            .NotNull().WithMessage("Student information is required")
            .SetValidator(studentValidator);

        RuleFor(x => x.School)
            .NotNull().WithMessage("School information is required")
            .SetValidator(schoolValidator);

        RuleFor(x => x.Internship)
            .NotNull().WithMessage("Internship details are required")
            .SetValidator(internshipValidator);

        // Cross-entity validation
        RuleFor(x => x)
            .Must(BeWithinSchoolCapacity).WithMessage("Request exceeds school's current capacity");
    }

    private static bool HaveRequiredDocuments(ICollection<RequirementsRegDto>? requirements)
    {
        if (requirements is null) return false;
        
        var requiredFiles = new[] { "resume", "consent", "waiver" };
        var fileNames = requirements.Select(r => r.FileName.ToLowerInvariant()).ToList();
        
        return requiredFiles.All(req => 
            fileNames.Any(f => f.Contains(req)));
    }

    private static bool BeConsistentGradeLevel(OnBoardingDto dto)
    {
        if (dto.Student?.GradeLevel == null || dto.Internship?.InternshipNature == null) 
            return true;

        return (dto.Student.GradeLevel, dto.Internship.InternshipNature) switch
        {
            (GradeLevelEnum.Grade11 or GradeLevelEnum.Grade12, 
             InternshipNatureEnum.WorkImmersion) => true,
            (GradeLevelEnum.CollegeFirstYear or GradeLevelEnum.CollegeSecondYear 
             or GradeLevelEnum.CollegeThirdYear or GradeLevelEnum.CollegeFourthYear, 
             InternshipNatureEnum.OJT) => true,
            _ => false
        };
    }

    private static bool BeWithinSchoolCapacity(OnBoardingDto dto) => 
        true; 
}
