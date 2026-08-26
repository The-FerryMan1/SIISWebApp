using System;
using System.IO;
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

        RuleFor(x => x.Student.GradeLevel)
            .NotNull().WithMessage("Grade level is required");

        RuleFor(x => x.Internship)
            .NotNull().WithMessage("Internship details are required")
            .SetValidator(internshipValidator);

        RuleFor(x => x.MoaFile)
            .NotNull().WithMessage("MOA file is required")
            .Must(file => file is not null && HaveValidExtension(file.FileName, ".pdf"))
            .WithMessage("MOA must be a PDF file");

        RuleFor(x => x.ResumeFile)
            .NotNull().WithMessage("Resume file is required")
            .Must(file => file is not null && HaveValidExtension(file.FileName, ".pdf", ".doc", ".docx"))
            .WithMessage("Resume must be a PDF, DOC, or DOCX file");

        RuleFor(x => x)
            .Must(BeWithinSchoolCapacity).WithMessage("Request exceeds school's current capacity");
    }

    private static bool HaveValidExtension(string fileName, params string[] allowedExtensions)
    {
        if (string.IsNullOrEmpty(fileName)) return false;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(ext);
    }

    private static bool BeWithinSchoolCapacity(OnBoardingDto dto) => 
        true; 
}
