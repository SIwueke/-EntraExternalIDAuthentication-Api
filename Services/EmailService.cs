using ExternalIdDemo.Api.Interfaces;
using ExternalIdDemo.Api.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ExternalIdDemo.Api.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(
        IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendMfaCodeAsync(
        string email,
        string code)
    {
        var message = new MailMessage
        {
            From = new MailAddress(
                _emailSettings.MailFrom),

            Subject =
                "Your verification code",

            Body =
                $"Your verification code is: {code}\n\n" +
                "This code will expire shortly.\n\n" +
                "If you did not request this code, " +
                "you can ignore this email.",

            IsBodyHtml = false
        };

        message.To.Add(email);

        using var client =
            new SmtpClient(
                _emailSettings.MailServer,
                _emailSettings.MailServerPort);

        if (!string.IsNullOrWhiteSpace(
                _emailSettings.Username) &&
            !string.IsNullOrWhiteSpace(
                _emailSettings.Password))
        {
            client.Credentials =
                new NetworkCredential(
                    _emailSettings.Username,
                    _emailSettings.Password);

            client.EnableSsl = true;
        }

        await client.SendMailAsync(message);
    }
}