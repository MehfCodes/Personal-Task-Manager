using System;
using PTM.Application.Interfaces.Providers;
using PTM.Application.Interfaces.Services;

namespace PTM.Application.Services;

public class EmailService : IEmailService
{
    private readonly ISmtpEmailSender emailSender;

    public EmailService(ISmtpEmailSender emailSender)
    {
        this.emailSender = emailSender;
    }
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        await emailSender.SendEmailAsync(to, subject, body);
    }
}
