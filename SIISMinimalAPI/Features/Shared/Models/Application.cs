using System.ComponentModel.DataAnnotations;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class Application
    {
        public long Id { get; set; }
        public Guid Uuid { get; set; } = Guid.NewGuid();
        public Guid ApplicationUUID { get; set; } = Guid.NewGuid();
        public ApplicationStatusEnum Status { get; set; } = ApplicationStatusEnum.Pending;
        public string? Reason { get; set; } = string.Empty;
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [DataType(DataType.Date)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public long StudentId { get; set; }
        public Student? Student { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}