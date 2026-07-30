using System;

namespace SIISMinimalAPI.Features.Report.RequirementsMissing;

public interface IMissingRequirementsService
{
    Task<byte[]> GetMissingRequirements(CancellationToken ct);
    Task<byte[]> GetMissingRequirementsCsv(CancellationToken ct);
}
