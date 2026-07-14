using System;

namespace SIISMinimalAPI.Features.Shared.Models;

public class RegistrationTokenModel
{
    public long Id { get; set; }
    public Guid Token { get; set; } = Guid.CreateVersion7();
    public DateTime ExpDate { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.Today;
        
}
