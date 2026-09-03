using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SIISMinimalAPI.Features.Email;

public class EmailService(IOptions<SmtpOptions> options) : IEmailService
{   
    private readonly SmtpOptions _options = options.Value;

     public async Task SendEmailAsync(string to, string subject, string htmlbody)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("App Notifications", _options.Username));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        // Build HTML or plain text body
        message.Body = new TextPart("html") { Text = htmlbody };

        using var client = new SmtpClient();
        
        await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_options.Username, _options.Password);
        
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
