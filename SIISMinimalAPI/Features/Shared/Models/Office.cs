using System.ComponentModel.DataAnnotations;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class Office
    {
        public long Id { get; set; }
        [Required, StringLength(255, MinimumLength = 1)]
        public string OfficeName { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public User? User { get; set; }
        public ICollection<Placement>? Placements { get; set; } = [];
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}