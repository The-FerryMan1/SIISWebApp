using System.ComponentModel.DataAnnotations;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class OfficeAccountModel
    {
        public long Id { get; set; }
        public long OfficeId { get; set; }
        [Required, StringLength(100, MinimumLength = 1)]
        public string Username { get; set; }
        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; }
        [Required, StringLength(255)]
        public string PasswordHash { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}