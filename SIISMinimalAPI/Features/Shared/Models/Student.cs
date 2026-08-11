using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class Student
    {
        public long Id { get; set; }
        public Guid StudentUUID { get; set; } = Guid.NewGuid();

        [Required, StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }

        [NotMapped]
        public int? Age
        {
            get
            {
                if (!DateOfBirth.HasValue)
                {
                    return null;
                }

                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - DateOfBirth.Value.Year;
                if (today < DateOfBirth.Value.AddYears(age))
                {
                    age--;
                }

                return age;
            }
        }

        [NotMapped]
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public GennderEnum Gender { get; set; }

        [Required, StringLength(255, MinimumLength = 1)]
        public string SchoolName { get; set; } = string.Empty;

        [Required]
        public string SchoolAddress { get; set; } = string.Empty;

        [Required, StringLength(255, MinimumLength = 1)]
        public string SchoolContactPerson { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string SchoolContactPersonEmail { get; set; } = string.Empty;

        [Required, Phone]
        public string SchoolContactPersonPhone { get; set; } = string.Empty;

        [Required]
        public GradeLevelEnum GradeLevel { get; set; }

        [StringLength(255)]
        public string? OtherGradeLevel { get; set; }

        [Required]
        public InternshipNatureEnum InternshipNature { get; set; }

        [StringLength(255)]
        public string? OtherInternshipNature { get; set; }

        [Required]
        public StrandEnum Strand { get; set; }

        [StringLength(255)]
        public string? OtherStrand { get; set; }

        [Required]
        public DegreeEnum Degree { get; set; }

        [StringLength(255)]
        public string? OtherDegree { get; set; }

        [Required]
        public int TotalInternshipHours { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Application? Application { get; set; }
        public Placement? Placement { get; set; }
        public ICollection<Requirement>? Requirements { get; set; } = [];
    }
}