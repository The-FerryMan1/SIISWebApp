using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Data
{
    public static class SeederStudent
    {
        public static async Task InitStudents(IServiceProvider service)
        {
            var dbContext = service.GetRequiredService<AppDbContext>();

            var count = await dbContext.Students.CountAsync();
            if (count > 0) return;

            var offices = await dbContext.Offices.ToListAsync();

            var students = new List<StudentModel>
            {
                new StudentModel
                {
                    Email = "john.smith@student.com",
                    LastName = "Smith",
                    FirstName = "John",
                    MiddleName = "A.",
                    ContactNumber = "09123456789",
                    Address = "123 Main St, Cavite City",
                    DateOfBirth = new DateOnly(2005, 3, 15),
                    Gender = GennderEnum.Male,
                    GradeLevel = GradeLevelEnum.Grade11,
                    OfficeId = offices.Count > 0 ? offices[0].Id : null,
                    School = new SchoolModel
                    {
                        Name = "Cavite National High School",
                        Address = "Trece Martires City, Cavite",
                        ContactPerson = "Maria Santos",
                        Email = "cnhs@cavite.gov.ph",
                        ContactNumber = "09876543210"
                    },
                    Internship = new InternshipModel
                    {
                        InternshipNature = InternshipNatureEnum.OJT,
                        Strand = StrandEnum.STEM,
                        Degree = DegreeEnum.BSIT,
                        StartDate = new DateOnly(2025, 6, 1),
                        EstimatedEndDate = new DateOnly(2025, 12, 1),
                        InternshipTotalHours = 400
                    },
                    Application = new ApplicationModel
                    {
                        ApplicationUUID = Guid.NewGuid(),
                        Status = ApplicationStatusEnum.Approved
                    },
                    Requirements =
                    [
                        new RequirementModel
                        {
                            FileName = "john_smith_id.pdf",
                            FilePath = "/uploads/Smith/john_smith_id.pdf",
                            FileType = "application/pdf"
                        },
                        new RequirementModel
                        {
                            FileName = "john_smith_clearance.pdf",
                            FilePath = "/uploads/Smith/john_smith_clearance.pdf",
                            FileType = "application/pdf"
                        }
                    ]
                },
                new StudentModel
                {
                    Email = "maria.clara@student.com",
                    LastName = "Clara",
                    FirstName = "Maria",
                    MiddleName = "D.",
                    ContactNumber = "09234567890",
                    Address = "456 Elm St, Tagaytay City",
                    DateOfBirth = new DateOnly(2004, 7, 22),
                    Gender = GennderEnum.Female,
                    GradeLevel = GradeLevelEnum.Grade12,
                    OfficeId = offices.Count > 1 ? offices[1].Id : null,
                    School = new SchoolModel
                    {
                        Name = "Tagaytay National High School",
                        Address = "Tagaytay City, Cavite",
                        ContactPerson = "Pedro Reyes",
                        Email = "tnhs@tagaytay.gov.ph",
                        ContactNumber = "09765432109"
                    },
                    Internship = new InternshipModel
                    {
                        InternshipNature = InternshipNatureEnum.Internship,
                        Strand = StrandEnum.ABM,
                        Degree = DegreeEnum.BSBA,
                        StartDate = new DateOnly(2025, 7, 15),
                        EstimatedEndDate = new DateOnly(2026, 1, 15),
                        InternshipTotalHours = 300
                    },
                    Application = new ApplicationModel
                    {
                        ApplicationUUID = Guid.NewGuid(),
                        Status = ApplicationStatusEnum.Pending
                    },
                    Requirements =
                    [
                        new RequirementModel
                        {
                            FileName = "maria_clara_id.pdf",
                            FilePath = "/uploads/Clara/maria_clara_id.pdf",
                            FileType = "application/pdf"
                        }
                    ]
                },
                new StudentModel
                {
                    Email = "robert.juan@student.com",
                    LastName = "Juan",
                    FirstName = "Robert",
                    MiddleName = "T.",
                    ContactNumber = "09345678901",
                    Address = "789 Oak Ave, Imus City",
                    DateOfBirth = new DateOnly(2003, 11, 8),
                    Gender = GennderEnum.Male,
                    GradeLevel = GradeLevelEnum.CollegeThirdYear,
                    OfficeId = offices.Count > 2 ? offices[2].Id : null,
                    School = new SchoolModel
                    {
                        Name = "Imus Institute of Science and Technology",
                        Address = "Imus City, Cavite",
                        ContactPerson = "Ana Delos Santos",
                        Email = "iist@imus.edu.ph",
                        ContactNumber = "09654321098"
                    },
                    Internship = new InternshipModel
                    {
                        InternshipNature = InternshipNatureEnum.Apprenticeship,
                        Degree = DegreeEnum.BSCS,
                        StartDate = new DateOnly(2025, 5, 1),
                        EstimatedEndDate = new DateOnly(2025, 11, 1),
                        InternshipTotalHours = 500
                    },
                    Application = new ApplicationModel
                    {
                        ApplicationUUID = Guid.NewGuid(),
                        Status = ApplicationStatusEnum.Rejected
                    },
                    Requirements =
                    [
                        new RequirementModel
                        {
                            FileName = "robert_juan_id.pdf",
                            FilePath = "/uploads/Juan/robert_juan_id.pdf",
                            FileType = "application/pdf"
                        },
                        new RequirementModel
                        {
                            FileName = "robert_juan_transcript.pdf",
                            FilePath = "/uploads/Juan/robert_juan_transcript.pdf",
                            FileType = "application/pdf"
                        },
                        new RequirementModel
                        {
                            FileName = "robert_juan_clearance.pdf",
                            FilePath = "/uploads/Juan/robert_juan_clearance.pdf",
                            FileType = "application/pdf"
                        }
                    ]
                },
                new StudentModel
                {
                    Email = "lorenza.ramos@student.com",
                    LastName = "Ramos",
                    FirstName = "Lorenza",
                    MiddleName = "M.",
                    ContactNumber = "09456789012",
                    Address = "321 Pine St, Kawit, Cavite",
                    DateOfBirth = new DateOnly(2006, 1, 30),
                    Gender = GennderEnum.Female,
                    GradeLevel = GradeLevelEnum.CollegeFirstYear,
                    OfficeId = offices.Count > 3 ? offices[3].Id : null,
                    School = new SchoolModel
                    {
                        Name = "Kawit National High School",
                        Address = "Kawit, Cavite",
                        ContactPerson = "Roberto Aguilar",
                        Email = "knhs@kawit.gov.ph",
                        ContactNumber = "09543210987"
                    },
                    Internship = new InternshipModel
                    {
                        InternshipNature = InternshipNatureEnum.WorkImmersion,
                        Degree = DegreeEnum.BSEd,
                        StartDate = new DateOnly(2025, 8, 1),
                        EstimatedEndDate = new DateOnly(2026, 2, 28),
                        InternshipTotalHours = 200
                    },
                    Application = new ApplicationModel
                    {
                        ApplicationUUID = Guid.NewGuid(),
                        Status = ApplicationStatusEnum.Pending
                    },
                    Requirements =
                    [
                        new RequirementModel
                        {
                            FileName = "lorenza_ramos_id.pdf",
                            FilePath = "/uploads/Ramos/lorenza_ramos_id.pdf",
                            FileType = "application/pdf"
                        }
                    ]
                },
                new StudentModel
                {
                    Email = "dennis.velasco@student.com",
                    LastName = "Velasco",
                    FirstName = "Dennis",
                    MiddleName = "R.",
                    ContactNumber = "09567890123",
                    Address = "654 Maple Dr, Bacoor City",
                    DateOfBirth = new DateOnly(2004, 9, 12),
                    Gender = GennderEnum.Male,
                    GradeLevel = GradeLevelEnum.CollegeSecondYear,
                    OfficeId = offices.Count > 4 ? offices[4].Id : null,
                    School = new SchoolModel
                    {
                        Name = "Bacoor National High School",
                        Address = "Bacoor City, Cavite",
                        ContactPerson = "Sonia Mendoza",
                        Email = "bnhs@bacoor.gov.ph",
                        ContactNumber = "09432109876"
                    },
                    Internship = new InternshipModel
                    {
                        InternshipNature = InternshipNatureEnum.OJT,
                        Degree = DegreeEnum.BSME,
                        StartDate = new DateOnly(2025, 6, 15),
                        EstimatedEndDate = new DateOnly(2025, 12, 15),
                        InternshipTotalHours = 350
                    },
                    Application = new ApplicationModel
                    {
                        ApplicationUUID = Guid.NewGuid(),
                        Status = ApplicationStatusEnum.Approved
                    },
                    Requirements =
                    [
                        new RequirementModel
                        {
                            FileName = "dennis_velasco_id.pdf",
                            FilePath = "/uploads/Velasco/dennis_velasco_id.pdf",
                            FileType = "application/pdf"
                        },
                        new RequirementModel
                        {
                            FileName = "dennis_velasco_photo.pdf",
                            FilePath = "/uploads/Velasco/dennis_velasco_photo.jpg",
                            FileType = "image/jpeg"
                        }
                    ]
                }
            };

            await dbContext.Students.AddRangeAsync(students);
            await dbContext.SaveChangesAsync();
        }
    }
}