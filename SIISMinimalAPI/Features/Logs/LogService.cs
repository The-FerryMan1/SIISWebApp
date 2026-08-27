using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Logs;

public interface ILogService
{
    Task WriteAsync(string action, string entity, long? entityId, string userId, string? details = null, CancellationToken ct = default);
}

public class LogService(AppDbContext context) : ILogService
{
    public async Task WriteAsync(string action, string entity, long? entityId, string userId, string? details = null, CancellationToken ct = default)
    {
        var log = new LogsModel
        {
            Action = action,
            Entity = entity,
            EntityId = entityId,
            UserId = userId,
            Details = details,
            CreateAt = DateTime.Now,
            IsDeleted = false
        };

        context.Logs.Add(log);
        await context.SaveChangesAsync(ct);
    }
}
