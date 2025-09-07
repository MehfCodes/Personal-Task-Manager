namespace PTM.Application.Interfaces.Providers;

public interface ISmtpEmailSender
{
   Task SendEmailAsync(string to, string subject, string body);
}
