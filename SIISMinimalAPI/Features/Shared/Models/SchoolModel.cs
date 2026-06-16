using System.ComponentModel.DataAnnotations;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class SchoolModel
    {
        public long Id { get; set; }
        [Required, StringLength(255, MinimumLength = 1)]
        public string Name { get; set; }
        [Required, StringLength(255, MinimumLength = 1)]
        public string Address { get; set; }
        [Required, StringLength(255, MinimumLength = 1)]
        public string ContactPerson { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, StringLength(11, MinimumLength = 11)]
        public string ContactNumber { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public long StudentId { get; set; }
        public StudentModel Student { get; set; }
    }
}
