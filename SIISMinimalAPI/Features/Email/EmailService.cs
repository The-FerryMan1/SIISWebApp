using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SIISMinimalAPI.Features.Email;

public class EmailService(IOptions<SmtpOptions> options, ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpOptions _options = options.Value;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task SendEmailAsync(string to, string subject, string htmlbody)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            _logger.LogWarning("SMTP Host is not configured. Email to {To} not sent.", to);
            return;
        }

        _logger.LogInformation("Sending email to {To} via {Host}:{Port}", to, _options.Host, _options.Port);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SIIS Notifications", _options.Username));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = htmlbody };

        using var client = new SmtpClient();
        
        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_options.Username, _options.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} via {Host}:{Port}. Error: {Error}", to, _options.Host, _options.Port, ex.Message);
            throw;
        }
    }
}
