namespace SIISMinimalAPI.Features.Inbox;

public class InboxItemDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string? OfficeName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ActionUrl { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}
