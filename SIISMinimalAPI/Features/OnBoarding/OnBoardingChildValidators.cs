using System;
using FluentValidation;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.OnBoarding;

public class StudentRegDtoValidator : AbstractValidator<StudentRegDto>
{
    public StudentRegDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Student email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z\s\-]+$").WithMessage("Last name contains invalid characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50);

        RuleFor(x => x.ContactNumber)
            .NotEmpty().WithMessage("Contact number is required")
            .Matches(@"^(\+63|0)\d{10}$").WithMessage("Invalid PH mobile format. Use +63XXXXXXXXXX or 0XXXXXXXXXX");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(200);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeAtLeast15YearsOld).WithMessage("Student must be at least 15 years old")
            .Must(BeNotOver60YearsOld).WithMessage("Invalid date of birth");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value");

        RuleFor(x => x.GradeLevel)
            .IsInEnum().WithMessage("Invalid grade level");
    }

    private static bool BeAtLeast15YearsOld(DateOnly dob) => 
        dob.AddYears(15) <= DateOnly.FromDateTime(DateTime.Today);

    private static bool BeNotOver60YearsOld(DateOnly dob) => 
        dob.AddYears(60) >= DateOnly.FromDateTime(DateTime.Today);
}

public class SchoolRegDtoValidator : AbstractValidator<SchoolRegDto>
{
    public SchoolRegDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("School name is required")
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("School address is required")
            .MaximumLength(200);

        RuleFor(x => x.ContactPerson)
            .NotEmpty().WithMessage("Contact person is required")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("School email is required")
            .EmailAddress().WithMessage("Invalid school email format");

        RuleFor(x => x.ContactNumber)
            .NotEmpty().WithMessage("School contact number is required")
            .Matches(@"^(\+63|0)\d{10}$").WithMessage("Invalid PH mobile format");
    }
}

public class InternshipRegDtoValidator : AbstractValidator<InternshipRegDto>
{
    public InternshipRegDtoValidator()
    {
        RuleFor(x => x.InternshipNature)
            .IsInEnum().WithMessage("Invalid internship nature");

        // Conditional: Strand required for SHS, Degree for College
        When(x => x.InternshipNature == InternshipNatureEnum.WorkImmersion, () =>
        {
            RuleFor(x => x.Strand)
                .NotNull().WithMessage("Strand is required for Senior High School internships")
                .IsInEnum().WithMessage("Invalid strand value");

            RuleFor(x => x.Degree)
                .Null().WithMessage("Degree should not be set for Senior High School");
        });
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddDays(7)))
            .WithMessage("Start date must be at least 7 days from today");

        RuleFor(x => x.EstimatedEndDate)
            .NotEmpty().WithMessage("Estimated end date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.InternshipTotalHours)
            .InclusiveBetween(80, 600)
            .WithMessage("Total hours must be between 80 and 600");
    }
}

public class RequirementsRegDtoValidator : AbstractValidator<RequirementsRegDto>
{
    private static readonly HashSet<string> AllowedExtensions = 
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };

    public RequirementsRegDtoValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255)
            .Must(HaveValidExtension).WithMessage("File type not allowed. Use: PDF, JPG, PNG, DOC, DOCX");

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required")
            .Must(BeValidPath).WithMessage("Invalid file path format");

        RuleFor(x => x.FileType)
            .NotEmpty().WithMessage("MIME type is required")
            .Must(BeValidMimeType).WithMessage("Invalid MIME type");
    }

    private static bool HaveValidExtension(string fileName) =>
        !string.IsNullOrEmpty(fileName) && 
        AllowedExtensions.Contains(Path.GetExtension(fileName));

    private static bool BeValidPath(string path) =>
        !string.IsNullOrEmpty(path) && 
        !path.Contains("..") && 
        !path.Contains("//");

    private static bool BeValidMimeType(string mime) =>
        mime.StartsWith("application/") || mime.StartsWith("image/");
}
