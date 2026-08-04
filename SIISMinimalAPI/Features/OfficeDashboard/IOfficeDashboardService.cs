using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.OfficeDashboard;

public interface IOfficeDashboardService
{
    Task<OfficeDashboardDto> GetDashboardAsync(long officeId, CancellationToken ct);
}