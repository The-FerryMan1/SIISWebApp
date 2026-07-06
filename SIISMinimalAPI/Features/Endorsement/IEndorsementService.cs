using System;
using QuestPDF.Fluent;
using SIISMinimalAPI.Features.Endorsement.Bulk;
using SIISMinimalAPI.Features.Endorsement.Create;

namespace SIISMinimalAPI.Features.Endorsement;

public interface IEndorsementService
{
    Task<Document?> GenerateEndorsement(Guid uuid, CancellationToken ct);
    Task<Document?> MultiOjtEndorsement(EndorsementBulkDto dto, CancellationToken ct);
}
