namespace SIISMinimalAPI.Features.SystemSettings;

public class SystemSettingsDto
{
    public string? LogoPath { get; set; }
    public string? ThemeColor { get; set; }
}

public class UpdateSystemSettingsRequest
{
    public string? LogoPath { get; set; }
    public string? ThemeColor { get; set; }
}
