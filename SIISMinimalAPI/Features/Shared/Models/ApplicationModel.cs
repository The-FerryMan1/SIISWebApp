using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class ApplicationModel
    {
        public long Id { get; set; }
        public Guid ApplicationUUID { get; set; }
        public ApplicationStatusEnum Status { get; set; } = ApplicationStatusEnum.Pending;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public long StudentId { get; set; }
        public StudentModel Student { get; set; }
    }
}
