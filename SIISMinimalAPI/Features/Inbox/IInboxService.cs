using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Inbox;

public interface IInboxService
{
    Task<List<InboxItemDto>> GetInboxForAdminAsync(CancellationToken ct);
    Task<List<InboxItemDto>> GetInboxForOfficeAsync(string userId, CancellationToken ct);
    Task<int> GetInboxCountAsync(string userId, bool isAdmin, CancellationToken ct);
}
