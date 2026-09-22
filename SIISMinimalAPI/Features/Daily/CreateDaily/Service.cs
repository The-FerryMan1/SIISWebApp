using SIISMinimalAPI.Data;

namespace SIISMinimalAPI.Features.Daily.CreateDaily;

public class Service(AppDbContext db) : IService
{   
    private readonly AppDbContext _db = db;
    public async Task<Request> CreateDailyAsync(Request request, CancellationToken ct)
    {
        _db.AddRange(request);
        await _db.SaveChangesAsync(ct);

        return request;
    }
}