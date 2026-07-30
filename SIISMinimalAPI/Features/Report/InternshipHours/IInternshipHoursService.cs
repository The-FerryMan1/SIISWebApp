using System;

namespace SIISMinimalAPI.Features.Report.InternshipHours;

public interface IInternshipHoursService
{
    Task<byte[]> GetInternshipHours(CancellationToken ct);
}
