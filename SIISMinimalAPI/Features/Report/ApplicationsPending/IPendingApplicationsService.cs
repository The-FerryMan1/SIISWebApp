using System;

namespace SIISMinimalAPI.Features.Report.ApplicationsPending;

public interface IPendingApplicationsService
{
    Task<byte[]> GetPendingApplications(CancellationToken ct);
}
