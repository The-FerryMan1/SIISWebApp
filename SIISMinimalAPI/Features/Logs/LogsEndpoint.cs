using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Logs;

public static class LogsEndpoint
{
    public static IEndpointRouteBuilder MapToLogs(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/logs")
            .WithTags("Logs")
            .RequireRateLimiting("standard")
            .RequireCors("AllowFrontend")
            .RequireAuthorization();

        group.MapGet("/", [Authorize] async Task<IResult>([AsParameters] PaginationQuery query, ClaimsPrincipal user, AppDbContext context, CancellationToken ct) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return TypedResults.Unauthorized();
            }

            var logsQuery = context.Logs
                .AsNoTracking()
                .Where(l => !l.IsDeleted)
                .OrderByDescending(l => l.CreateAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var lowerSearch = query.Search.ToLower();
                logsQuery = logsQuery.Where(l =>
                    l.Action.ToLower().Contains(lowerSearch) ||
                    l.Entity.ToLower().Contains(lowerSearch) ||
                    (l.Details != null && l.Details.ToLower().Contains(lowerSearch)) ||
                    l.UserId.ToLower().Contains(lowerSearch));
            }

            var total = await logsQuery.CountAsync(ct);
            var logs = await logsQuery
                .Skip((query.ValidatedPage - 1) * query.ValidatedPageSize)
                .Take(query.ValidatedPageSize)
                .Select(l => new LogDto
                {
                    Id = l.Id,
                    Action = l.Action,
                    Entity = l.Entity,
                    EntityId = l.EntityId,
                    UserId = l.UserId,
                    Details = l.Details,
                    CreatedAt = l.CreateAt
                })
                .ToListAsync(ct);

            return TypedResults.Ok(new PagedResult<LogDto>
            {
                Data = logs,
                Total = total,
                Page = query.ValidatedPage,
                PageSize = query.ValidatedPageSize
            });
        });

        return app;
    }
}

public class LogDto
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public long? EntityId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
