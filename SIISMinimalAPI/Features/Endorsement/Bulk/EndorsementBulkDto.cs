using System;

namespace SIISMinimalAPI.Features.Endorsement.Bulk;

public class EndorsementBulkDto
{
    public string? Office { get; set; }
    public ICollection<Guid> UUIDS { get; set; } = [];
}

public class EndorsementBySchoolDto
{
    public string SchoolName { get; set; } = string.Empty;
}
