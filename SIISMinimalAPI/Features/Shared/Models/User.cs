using Microsoft.AspNetCore.Identity;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class User : IdentityUser
    {
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
    }
}