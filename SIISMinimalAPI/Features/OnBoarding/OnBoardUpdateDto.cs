using System;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.OnBoarding;

public class OnBoardUpdateDto
{
    public StudentRegDto Student { get; set; }
    public SchoolRegDto School { get; set; }
    public InternshipRegDto Internship { get; set; }
    public ICollection<RequirementsRegDto> Requirements { get; set; }
    public OfficeUpdateDto Office { get; set; }
}



// student registration dto
public class StudentUpdateDto
{
    public string Email { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; } = string.Empty;
    public string ContactNumber { get; set; }
    public string Address { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public GennderEnum Gender { get; set; }
    public GradeLevelEnum GradeLevel { get; set; }
}

//school reg dto
public class SchoolUpdateDto
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string ContactPerson { get; set; }
    public string Email { get; set; }
    public string ContactNumber { get; set; }
}

//internship req dto
public class InternshipUpdateDto
{
    public InternshipNatureEnum InternshipNature { get; set; }
    public StrandEnum? Strand { get; set; }
    public DegreeEnum? Degree { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EstimatedEndDate { get; set; }
    public int InternshipTotalHours { get; set; }

}

//requirements Update dto
public class RequirementsUpdateDto
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileType { get; set; }
}

public class OfficeUpdateDto
{
    public OfficeNameEnum Name { get; set; }
}

