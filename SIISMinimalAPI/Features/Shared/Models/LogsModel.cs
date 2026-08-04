using System.ComponentModel.DataAnnotations;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class LogsModel
    {
        public long Id { get; set; }
        [Required, StringLength(50, MinimumLength = 1)]
        public string Action { get; set; } = string.Empty;
        [Required, StringLength(255, MinimumLength = 1)]
        public string Entity { get; set; } = string.Empty;
        public long? EntityId { get; set; }
        [Required, StringLength(255, MinimumLength = 1)]
        public string UserId { get; set; } = string.Empty;
        [StringLength(1000)]
        public string? Details { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}