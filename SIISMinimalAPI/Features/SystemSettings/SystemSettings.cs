namespace SIISMinimalAPI.Features.SystemSettings;

public class SystemSettings
{
    public int Id { get; set; } = 1;
    public string? LogoPath { get; set; }
    public string? ThemeColor { get; set; } = "#2f5cba";
    public DateTime? UpdatedAt { get; set; }
}
