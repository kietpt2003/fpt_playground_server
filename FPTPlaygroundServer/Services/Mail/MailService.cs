
using FPTPlaygroundServer.Common.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FPTPlaygroundServer.Services.Mail;

public class MailService(IOptions<MailSettings> mailSettings)
{
    private readonly MailSettings _mailSettings = mailSettings.Value;

    public async Task SendMail(string subject, string mailTo, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("FPT Playground", _mailSettings.Mail));
        message.To.Add(new MailboxAddress("", mailTo));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}
