using System.ComponentModel.DataAnnotations;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class Registration
    {
        public long Id { get; set; }
        [Required, StringLength(100, MinimumLength = 1)]
        public string Token { get; set; } = string.Empty;
        [Required]
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}