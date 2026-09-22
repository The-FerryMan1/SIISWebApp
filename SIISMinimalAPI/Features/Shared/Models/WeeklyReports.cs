namespace SIISMinimalAPI.Features.Shared.Models
{
    public class WeeklyReport
    {
          public int Id { get; set; }

          public long ProgressId { get; set; }
          public DateOnly WeekStartDate { get; set; }
          public DateOnly WeekEndDate { get; set; }
          public ICollection<DailyReport>? DailyReport { get; set; }
          public Progress? Progress { get; set; }
    }
}