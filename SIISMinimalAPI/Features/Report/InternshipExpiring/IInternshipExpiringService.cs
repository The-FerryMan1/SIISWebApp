using System;

namespace SIISMinimalAPI.Features.Report.InternshipExpiring;

public interface IInternshipExpiringService
{
    Task<byte[]> GetExpiringInternships(CancellationToken ct, int daysThreshold = 30);
    Task<byte[]> GetExpiringInternshipsCsv(CancellationToken ct, int daysThreshold = 30);
}
