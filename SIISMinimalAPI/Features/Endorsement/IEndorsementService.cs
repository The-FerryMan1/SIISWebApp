using System;
using QuestPDF.Fluent;
using SIISMinimalAPI.Features.Endorsement.Bulk;
using SIISMinimalAPI.Features.Endorsement.Create;

namespace SIISMinimalAPI.Features.Endorsement;

public interface IEndorsementService
{
    Task<Document?> GenerateEndorsement(Guid uuid, string currentUserId, CancellationToken ct);
    Task<Document?> MultiOjtEndorsement(EndorsementBulkDto dto, string currentUserId, CancellationToken ct);
    Task<Document?> GenerateEndorsementBySchool(string schoolName, string? office, string currentUserId, CancellationToken ct);
}
