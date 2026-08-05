using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SIISMinimalAPI.Features.Application;

public class ApplicationDto
{
    public long Id { get; set; }
    public Guid ApplicationUUID { get; set; }
    public string FullName {get; set;}
    public string Status {get; set;}
    public string DegreeStrand { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? OfficeName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
