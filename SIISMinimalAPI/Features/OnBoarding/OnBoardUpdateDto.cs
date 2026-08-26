using System;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.OnBoarding;

public class OnBoardUpdateDto
{
    public StudentUpdateDto? Student { get; set; }
    public SchoolUpdateDto? School { get; set; }
    public InternshipUpdateDto? Internship { get; set; }
    public ICollection<RequirementsUpdateDto>? Requirements { get; set; }
    public OfficeUpdateDto? Office { get; set; }
     public IFormFileCollection? Files { get; set; }
    public IFormFile? MoaFile { get; set; }
    public IFormFile? ResumeFile { get; set; }
}



// student registration dto
public class StudentUpdateDto
{
    public string? Email { get; set; }
    public string? LastName { get; set; } 
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Address { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public GennderEnum Gender { get; set; }
    public GradeLevelEnum GradeLevel { get; set; }
}

//school reg dto
public class SchoolUpdateDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? ContactNumber { get; set; }
}

//internship req dto
public class InternshipUpdateDto
{
    public InternshipNatureEnum InternshipNature { get; set; }
    public StrandEnum? Strand { get; set; }
    public DegreeEnum? Degree { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EstimatedEndDate { get; set; }
    public int InternshipTotalHours { get; set; }
    public int AccumulatedHours { get; set; }

}

//requirements Update dto
public class RequirementsUpdateDto
{
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public RequirementTypeEnum RequirementTypeEnum { get; set; }
}

public class OfficeUpdateDto
{
    public string Name { get; set; } = string.Empty;
}
