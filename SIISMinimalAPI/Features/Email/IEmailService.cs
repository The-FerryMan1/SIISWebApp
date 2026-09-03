using System;

namespace SIISMinimalAPI.Features.Email;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlbody);
}
