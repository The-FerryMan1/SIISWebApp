using System;

namespace SIISMinimalAPI.Features.RegistrationToken;

public class RegistrationTokenDto
{
    public long Id { get; set; }
    public Guid Uuid { get; set; }
    public DateTime ExpDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GenerateRegistrationTokenDto
{
    public DateTime ExpDate { get; set; }
}

public class ExtendRegistrationTokenDto
{
    public DateTime ExtendedDate { get; set; }
}
