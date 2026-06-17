using SIISMinimalAPI.Features.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class StudentModel
    {
        public long Id { get; set; }
        public Guid StudentUUID { get; set; } = Guid.NewGuid();
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(255, MinimumLength = 1)]
        public string LastName { get; set; }

        [Required, StringLength(255, MinimumLength = 1)]
        public string FirstName { get; set; }

        [StringLength(255)]
        public string MiddleName { get; set; } = string.Empty;

        [Required, StringLength(11, MinimumLength = 11)]
        public string ContactNumber   { get; set; }

        [Required, StringLength(255, MinimumLength = 1)]
        public string Address { get; set; }

        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }

        [Required, EnumDataType(typeof(GennderEnum))]

        public GennderEnum Gender { get; set; }
        [Required, EnumDataType(typeof(GradeLevelEnum))]
        public GradeLevelEnum GradeLevel { get; set; }


        public bool IsDeleted { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }



        //navigations
        public SchoolModel School { get; set; }
        public InternshipModel Internship { get; set; }
        public ApplicationModel Application { get; set; }
        public ICollection<RequirementModel> Requirements { get; set; } = [];

        public long? OfficeId { get; set; }
        public OfficeModel Office { get; set; }
    }
}
