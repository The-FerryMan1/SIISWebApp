namespace SIISMinimalAPI.Features.Shared.Models
{
    public class Otp
    {
        public int Id { get; set; }
        public string HashToken { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public DateTime Exp {get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; }
    }
}