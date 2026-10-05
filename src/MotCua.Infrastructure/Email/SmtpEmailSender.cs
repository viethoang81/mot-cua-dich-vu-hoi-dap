using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MotCua.Domain.Interfaces;

namespace MotCua.Infrastructure.Email;

public class SmtpSettings
{
    public string Host        { get; set; } = "smtp.gmail.com";
    public int    Port        { get; set; } = 587;
    public bool   UseSsl      { get; set; } = false;
    public string UserName    { get; set; } = default!;
    public string Password    { get; set; } = default!;
    public string DisplayName { get; set; } = "Mot Cua HUCE";
}

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _cfg;
    public SmtpEmailSender(IOptions<SmtpSettings> opts) => _cfg = opts.Value;

    public async Task GuiAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_cfg.DisplayName, _cfg.UserName));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_cfg.Host, _cfg.Port,
            _cfg.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable, ct);
        await client.AuthenticateAsync(_cfg.UserName, _cfg.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
