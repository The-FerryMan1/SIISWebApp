namespace SIISMinimalAPI.Features.Daily.CreateDaily;

public interface IService
{
    Task<Request> CreateDailyAsync(Request request, CancellationToken ct);
}