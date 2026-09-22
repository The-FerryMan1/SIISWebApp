namespace SIISMinimalAPI.Features.Shared.Models
{
    public class WeeklyReport
    {
          public int Id { get; set; }

          public long ProgressId { get; set; }
          public ICollection<DailyReport>? DailyReport { get; set; }
          public Progress? Progress { get; set; }
    }
}