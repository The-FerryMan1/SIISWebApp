using System;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Endorsement.Bulk;

public class EndorsementBulkDto
{
    public OfficeNameEnum Office { get; set; }
    public ICollection<Guid> UUIDS { get; set; } = [];
}

