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

            var students = new List<Student>
            {
                new Student
                {
                    Email = "john.smith@student.com",
                    LastName = "Smith",
                    FirstName = "John",
                    MiddleName = "A.",
                    ContactNumber = "09123456789",
                    Address = "123 Main St, Cavite City",
                    DateOfBirth = new DateOnly(2005, 3, 15),
                    Gender = GennderEnum.Male,
                    GradeLevel = GradeLevelEnum.SeniorHighSchool,
                    SchoolName = "Cavite National High School",
                    SchoolAddress = "Trece Martires City, Cavite",
                    SchoolContactPerson = "Maria Santos",
                    SchoolContactPersonEmail = "cnhs@cavite.gov.ph",
                     SchoolContactPersonPhone = "09876543210",
                     InternshipNature = InternshipNatureEnum.OnTheJobTraining,
                     Strand = StrandEnum.STEM,
                    Degree = DegreeEnum.BSIT,
                    TotalInternshipHours = 400,
                    Application = new Application
                    {
                        Uuid = Guid.NewGuid(),
                        Status = ApplicationStatusEnum.Approved,
                    },
                    Requirements =
                    [
                        new Requirement { FileName = "john_smith_id.pdf", FilePath = "/uploads/Smith/john_smith_id.pdf", FileType = "application/pdf" },
                        new Requirement { FileName = "john_smith_clearance.pdf", FilePath = "/uploads/Smith/john_smith_clearance.pdf", FileType = "application/pdf" }
                    ]
                },
                new Student
                {
                    Email = "maria.clara@student.com",
                    LastName = "Clara",
                    FirstName = "Maria",
                    MiddleName = "D.",
                    ContactNumber = "09234567890",
                    Address = "456 Elm St, Tagaytay City",
                    DateOfBirth = new DateOnly(2004, 7, 22),
                    Gender = GennderEnum.Female,
                    GradeLevel = GradeLevelEnum.SeniorHighSchool,
                    SchoolName = "Tagaytay National High School",
                    SchoolAddress = "Tagaytay City, Cavite",
                    SchoolContactPerson = "Pedro Reyes",
                    SchoolContactPersonEmail = "tnhs@tagaytay.gov.ph",
                     SchoolContactPersonPhone = "09765432109",
                     InternshipNature = InternshipNatureEnum.OnTheJobTraining,
                     Strand = StrandEnum.ABM,
                    Degree = DegreeEnum.BSBA,
                    TotalInternshipHours = 300,
                    Application = new Application
                    {
                        Uuid = Guid.NewGuid(),
                        Status = ApplicationStatusEnum.Pending,
                    },
                    Requirements =
                    [
                        new Requirement { FileName = "maria_clara_id.pdf", FilePath = "/uploads/Clara/maria_clara_id.pdf", FileType = "application/pdf" }
                    ]
                },
                new Student
                {
                    Email = "robert.juan@student.com",
                    LastName = "Juan",
                    FirstName = "Robert",
                    MiddleName = "T.",
                    ContactNumber = "09345678901",
                    Address = "789 Oak Ave, Imus City",
                    DateOfBirth = new DateOnly(2003, 11, 8),
                    Gender = GennderEnum.Male,
                    GradeLevel = GradeLevelEnum.College,
                    SchoolName = "Imus Institute of Science and Technology",
                    SchoolAddress = "Imus City, Cavite",
                    SchoolContactPerson = "Ana Delos Santos",
                    SchoolContactPersonEmail = "iist@imus.edu.ph",
                     SchoolContactPersonPhone = "09654321098",
                     InternshipNature = InternshipNatureEnum.OnTheJobTraining,
                     Strand = StrandEnum.ICT,
                    Degree = DegreeEnum.BSCS,
                    TotalInternshipHours = 500,
                    Application = new Application
                    {
                        Uuid = Guid.NewGuid(),
                        Status = ApplicationStatusEnum.Rejected,
                    },
                    Requirements =
                    [
                        new Requirement { FileName = "robert_juan_id.pdf", FilePath = "/uploads/Juan/robert_juan_id.pdf", FileType = "application/pdf" },
                        new Requirement { FileName = "robert_juan_transcript.pdf", FilePath = "/uploads/Juan/robert_juan_transcript.pdf", FileType = "application/pdf" },
                        new Requirement { FileName = "robert_juan_clearance.pdf", FilePath = "/uploads/Juan/robert_juan_clearance.pdf", FileType = "application/pdf" }
                    ]
                },
                new Student
                {
                    Email = "lorenza.ramos@student.com",
                    LastName = "Ramos",
                    FirstName = "Lorenza",
                    MiddleName = "M.",
                    ContactNumber = "09456789012",
                    Address = "321 Pine St, Kawit, Cavite",
                    DateOfBirth = new DateOnly(2006, 1, 30),
                    Gender = GennderEnum.Female,
                    GradeLevel = GradeLevelEnum.College,
                    SchoolName = "Kawit National High School",
                    SchoolAddress = "Kawit, Cavite",
                    SchoolContactPerson = "Roberto Aguilar",
                    SchoolContactPersonEmail = "knhs@kawit.gov.ph",
                    SchoolContactPersonPhone = "09543210987",
                    InternshipNature = InternshipNatureEnum.WorkImmersion,
                    Strand = StrandEnum.HUMSS,
                    Degree = DegreeEnum.BSEd,
                    TotalInternshipHours = 200,
                    Application = new Application
                    {
                        Uuid = Guid.NewGuid(),
                        Status = ApplicationStatusEnum.Pending,
                    },
                    Requirements =
                    [
                        new Requirement { FileName = "lorenza_ramos_id.pdf", FilePath = "/uploads/Ramos/lorenza_ramos_id.pdf", FileType = "application/pdf" }
                    ]
                },
                new Student
                {
                    Email = "dennis.velasco@student.com",
                    LastName = "Velasco",
                    FirstName = "Dennis",
                    MiddleName = "R.",
                    ContactNumber = "09567890123",
                    Address = "654 Maple Dr, Bacoor City",
                    DateOfBirth = new DateOnly(2004, 9, 12),
                    Gender = GennderEnum.Male,
                    GradeLevel = GradeLevelEnum.College,
                    SchoolName = "Bacoor National High School",
                    SchoolAddress = "Bacoor City, Cavite",
                    SchoolContactPerson = "Sonia Mendoza",
                    SchoolContactPersonEmail = "bnhs@bacoor.gov.ph",
                     SchoolContactPersonPhone = "09432109876",
                     InternshipNature = InternshipNatureEnum.OnTheJobTraining,
                     Strand = StrandEnum.GAS,
                    Degree = DegreeEnum.BSME,
                    TotalInternshipHours = 350,
                    Application = new Application
                    {
                        Uuid = Guid.NewGuid(),
                        Status = ApplicationStatusEnum.Approved,
                    },
                    Requirements =
                    [
                        new Requirement { FileName = "dennis_velasco_id.pdf", FilePath = "/uploads/Velasco/dennis_velasco_id.pdf", FileType = "application/pdf" },
                        new Requirement { FileName = "dennis_velasco_photo.pdf", FilePath = "/uploads/Velasco/dennis_velasco_photo.jpg", FileType = "image/jpeg" }
                    ]
                }
            };

            await dbContext.Students.AddRangeAsync(students);
            await dbContext.SaveChangesAsync();
        }
    }
}