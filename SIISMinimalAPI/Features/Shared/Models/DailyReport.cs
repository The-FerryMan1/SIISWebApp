namespace SIISMinimalAPI.Features.Shared.Models
{
    public class DailyReport
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public required string Activities { get; set; }
        public int Hours { get; set; }
        public string? InCharge { get; set; }

        public int WeeklyReportId { get; set; }
        public WeeklyReport? WeeklyReport { get; set; }
        public string? Remarks { get; set; }
        public string? IncidentReport { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}