using System;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Application.GetById;

public class ApplicationGetByIdDto
{
    public ApplicationInfo? Application { get; set; }
    public StudentInfo? Student { get; set; }
    public SchoolInfo? School { get; set; }
    public InternshipInfo? Internship { get; set; }
    public ICollection<RequirementInfo>? Requirements { get; set; }
    public OfficeInfo? Office { get; set; }

}

public class StudentInfo
{
    public long Id { get; set; }
    public Guid StudentUUID { get; set; } = Guid.NewGuid();
    public string Email { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; } = string.Empty;
    public string ContactNumber { get; set; }
    public string Address { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public GennderEnum Gender { get; set; }
    public GradeLevelEnum GradeLevel { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    //navigations
    public long? OfficeId { get; set; }
}

public class SchoolInfo
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string ContactPerson { get; set; }
    public string Email { get; set; }
    public string ContactNumber { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

}

public class InternshipInfo
{
    public long Id { get; set; }

    public InternshipNatureEnum InternshipNature { get; set; }

    public StrandEnum? Strand { get; set; }

    public DegreeEnum? Degree { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EstimatedEndDate { get; set; }
    public int InternshipTotalHours { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class RequirementInfo
{
    public long Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileType { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class ApplicationInfo
{
        public long Id { get; set; }
        public Guid ApplicationUUID { get; set; }
        public ApplicationStatusEnum Status { get; set; } = ApplicationStatusEnum.Pending;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
}

public class OfficeInfo
{
        public long Id { get; set; }
        public OfficeNameEnum Name { get; set; }
        public string? CurrentOIC { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
}

