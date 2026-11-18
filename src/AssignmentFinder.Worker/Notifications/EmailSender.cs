using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AssignmentFinder.Worker.Notifications;

public class EmailSender(IOptions<EmailOptions> options)
{
    public async Task SendAsync(string toAddress, string subject, string bodyHtml, CancellationToken ct)
    {
        var settings = options.Value;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(toAddress));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = bodyHtml }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(settings.Host, settings.Port, settings.UseSsl, ct);
        if (!string.IsNullOrEmpty(settings.Username))
        {
            await client.AuthenticateAsync(settings.Username, settings.Password, ct);
        }
        await client.SendAsync(new SendRequest(message), ct);
        await client.DisconnectAsync(true, ct);
    }
}
