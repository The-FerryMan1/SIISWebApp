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
        RuleFor(x => x.Student.GradeLevel)
            .NotNull().WithMessage("Grade level is required");

        RuleFor(x => x.Internship)
            .NotNull().WithMessage("Internship details are required")
            .SetValidator(internshipValidator);

        RuleFor(x => x)
            .Must(BeWithinSchoolCapacity).WithMessage("Request exceeds school's current capacity");
    }

    private static bool BeWithinSchoolCapacity(OnBoardingDto dto) => 
        true; 
}
