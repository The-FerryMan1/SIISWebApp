using System.ComponentModel.DataAnnotations;

namespace SIISMinimalAPI.Features.Shared.Models;

public class OfficeNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long OfficeId { get; set; }
    public Office? Office { get; set; }

    [Required, StringLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string ActionUrl { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Priority { get; set; } = "Medium";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ReadAt { get; set; }
}