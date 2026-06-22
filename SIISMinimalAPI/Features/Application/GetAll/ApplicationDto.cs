using System;

namespace SIISMinimalAPI.Features.Application;

public class ApplicationDto
{
    public long Id { get; set; }
    public Guid ApplicationUUID { get; set; }
    public string FullName {get; set;}
    public string Status {get; set;}
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
