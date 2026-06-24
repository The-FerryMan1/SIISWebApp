using FluentValidation;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.OnBoarding;

public class OnBoardUpdateDtoValidator : AbstractValidator<OnBoardUpdateDto>
{
    public OnBoardUpdateDtoValidator(
        StudentUpdateDtoValidator studentValidator,
        SchoolUpdateDtoValidator schoolValidator,
        InternshipUpdateDtoValidator internshipValidator,
        RequirementsUpdateDtoValidator requirementsValidator,
        OfficeUpdateDtoValidator officeValidator)
    {
        RuleFor(x => x.Student)
            .NotNull()
            .SetValidator((FluentValidation.Validators.IPropertyValidator)studentValidator);

        RuleFor(x => x.School)
            .NotNull()
            .SetValidator((FluentValidation.Validators.IPropertyValidator)schoolValidator);

        RuleFor(x => x.Internship)
            .NotNull()
            .SetValidator((FluentValidation.Validators.IPropertyValidator)internshipValidator);

        RuleFor(x => x.Requirements)
            .NotNull()
            .ForEach(req => req.SetValidator((FluentValidation.Validators.IPropertyValidator)requirementsValidator));

        RuleFor(x => x.Office)
            .NotNull()
            .SetValidator(officeValidator);
    }
}

public class StudentUpdateDtoValidator : AbstractValidator<StudentUpdateDto>
{
    public StudentUpdateDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.MiddleName)
            .MaximumLength(50);

        RuleFor(x => x.ContactNumber)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[\d\s\+\-\(\)]+$")
            .WithMessage("Contact number contains invalid characters");

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(BeAtLeast12YearsOld)
            .WithMessage("Student must be at least 12 years old")
            .Must(BeNotInFuture)
            .WithMessage("Date of birth cannot be in the future");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Invalid gender value");

        RuleFor(x => x.GradeLevel)
            .IsInEnum()
            .WithMessage("Invalid grade level");
    }

    private static bool BeAtLeast12YearsOld(DateOnly dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.Today.AddYears(-age))) age--;
        return age >= 12;
    }

    private static bool BeNotInFuture(DateOnly dateOfBirth)
    {
        return dateOfBirth <= DateOnly.FromDateTime(DateTime.Today);
    }
}

public class SchoolUpdateDtoValidator : AbstractValidator<SchoolUpdateDto>
{
    public SchoolUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ContactPerson)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(x => x.ContactNumber)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[\d\s\+\-\(\)]+$")
            .WithMessage("Contact number contains invalid characters");
    }
}

public class InternshipUpdateDtoValidator : AbstractValidator<InternshipUpdateDto>
{
    public InternshipUpdateDtoValidator()
    {
        RuleFor(x => x.InternshipNature)
            .IsInEnum()
            .WithMessage("Invalid internship nature");

        RuleFor(x => x.Strand)
            .IsInEnum()
            .When(x => x.Strand.HasValue)
            .WithMessage("Invalid strand value");

        RuleFor(x => x.Degree)
            .IsInEnum()
            .When(x => x.Degree.HasValue)
            .WithMessage("Invalid degree value");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .Must(BeNotInPast)
            .WithMessage("Start date cannot be in the past")
            .Must(BeWithinOneYear)
            .WithMessage("Start date must be within one year from today");

        RuleFor(x => x.EstimatedEndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.InternshipTotalHours)
            .NotEmpty()
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .WithMessage("Total hours must be between 1 and 1000");

        RuleFor(x => x)
            .Must(HaveValidDuration)
            .WithMessage("Internship duration exceeds reasonable limits for the given hours");
    }

    private static bool BeNotInPast(DateOnly startDate)
    {
        return startDate >= DateOnly.FromDateTime(DateTime.Today);
    }

    private static bool BeWithinOneYear(DateOnly startDate)
    {
        return startDate <= DateOnly.FromDateTime(DateTime.Today.AddYears(1));
    }

    private static bool HaveValidDuration(InternshipUpdateDto dto)
    {
        if (dto.StartDate == default || dto.EstimatedEndDate == default)
            return true;

        var duration = dto.EstimatedEndDate.DayNumber - dto.StartDate.DayNumber;
        var maxDays = (dto.InternshipTotalHours / 4) + 30; // Allow some buffer
        return duration <= maxDays;
    }
}

public class RequirementsUpdateDtoValidator : AbstractValidator<RequirementsUpdateDto>
{
    public RequirementsUpdateDtoValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.FilePath)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeValidPath)
            .WithMessage("Invalid file path");

        RuleFor(x => x.FileType)
            .NotEmpty()
            .MaximumLength(50)
            .Must(BeAllowedFileType)
            .WithMessage("File type must be pdf, doc, docx, jpg, jpeg, or png");
    }

    private static bool BeValidPath(string path)
    {
        return !string.IsNullOrWhiteSpace(path) 
            && !path.Contains("..") 
            && !path.Contains("<");
    }

    private static bool BeAllowedFileType(string fileType)
    {
        var allowed = new[] { "pdf", "doc", "docx", "jpg", "jpeg", "png" };
        return allowed.Contains(fileType.ToLower());
    }
}

public class OfficeUpdateDtoValidator : AbstractValidator<OfficeUpdateDto>
{
    public OfficeUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .IsInEnum()
            .WithMessage("Invalid office name");
    }
}