namespace SIISMinimalAPI.Features.OfficeAccounts;

public class GetOfficeAccountDto
{
    public long Id { get; set; }
    public long OfficeId { get; set; }
    public string OfficeName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateOfficeAccountDto
{
    public long OfficeId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UpdateOfficeAccountDto
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }
}