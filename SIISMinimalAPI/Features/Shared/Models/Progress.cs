namespace SIISMinimalAPI.Features.Shared.Models
{
    public class Progress{
        public long Id { get; set; }
        public int TrainingHoursRendered { get; set; } = 0;
        public int TrainingHoursForWeek { get; set; } = 0;
        public int RemainingHours {get;set;} = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public long PlacementId { get; set; }
        public Placement? Placement { get; set; }    
    }
}
