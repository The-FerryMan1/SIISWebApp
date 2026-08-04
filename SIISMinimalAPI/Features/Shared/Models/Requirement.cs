using System.ComponentModel.DataAnnotations;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class Requirement
    {
        public long Id { get; set; }
        [Required, StringLength(255, MinimumLength = 1)]
        public string FileName { get; set; } = string.Empty;
        [Required]
        public string FilePath { get; set; } = string.Empty;
        [Required]
        public string FileType { get; set; } = string.Empty;
        public long StudentId { get; set; }
        public Student? Student { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}