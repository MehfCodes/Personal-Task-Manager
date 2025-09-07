using System;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using PTM.Application.Interfaces.Providers;

namespace PTM.Infrastructure.Providers.Email;

public class SmtpEmailSender : ISmtpEmailSender
{
    private readonly SmtpSettings smtp;

    public SmtpEmailSender(IOptions<SmtpSettings> smtpOptions)
    {
        smtp = smtpOptions.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(smtp.Email));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart("plain") { Text = body };

        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(smtp.Host, smtp.Port, smtp.EnableSSL);
        if (!string.IsNullOrEmpty(smtp.Password))
            await smtpClient.AuthenticateAsync(smtp.Email, smtp.Password);

        await smtpClient.SendAsync(email);
        await smtpClient.DisconnectAsync(true);
    }
}
