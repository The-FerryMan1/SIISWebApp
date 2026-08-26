using System.ComponentModel.DataAnnotations;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class Placement
    {
        public long Id { get; set; }
        public int AccumulatedHours { get; set; } = 0;
        [DataType(DataType.Date)]
         public PlacementStatusEnum PlacementStatus { get; set; } = PlacementStatusEnum.Ongoing;
        public DateOnly StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateOnly EstimatedEndDate { get; set; }
        public long OfficeId { get; set; }
        public Office? Office { get; set; }
        public long StudentId { get; set; }
        public Student? Student { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<Progress?>? Progresses { get; set; }
    }
}