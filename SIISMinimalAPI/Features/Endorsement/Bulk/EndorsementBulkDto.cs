using System;

namespace SIISMinimalAPI.Features.Endorsement.Bulk;

public class EndorsementBulkDto
{
    public string Office { get; set; } = string.Empty;
    public ICollection<Guid> UUIDS { get; set; } = [];
}
