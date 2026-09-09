namespace SIISMinimalAPI.Features.SystemSettings;

public interface ISystemSettingsService
{
    Task<SystemSettingsDto> GetSettingsAsync(CancellationToken ct);
    Task<SystemSettingsDto> UpdateSettingsAsync(UpdateSystemSettingsRequest request, CancellationToken ct);
}
