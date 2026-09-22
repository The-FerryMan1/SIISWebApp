namespace SIISMinimalAPI.Features.Daily.CreateDaily;

public record Schema
{
    public DateOnly Date { get; set; }
    public required string Activities { get; set; }
    public int Hours { get; set; }
    public string? InCharge { get; set; }
    public string? Remarks { get; set; }
    public string? IncidentReport { get; set; }
}

public record Request
{
     public required ICollection<Schema> Dailies { get; set; }
}