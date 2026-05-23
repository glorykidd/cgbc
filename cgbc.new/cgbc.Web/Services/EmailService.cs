using cgbc.Web.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace cgbc.Web.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAdminNotificationAsync(ConnectionCard card)
    {
        var adminEmail = _config["Email:AdminNotificationAddress"];
        if (string.IsNullOrWhiteSpace(adminEmail))
            return;

        var subject = $"New Connection Card: {card.Name}";
        var body = BuildAdminNotificationHtml(card);

        await SendAsync(adminEmail, "CGBC Admin", subject, body);
    }

    public async Task ForwardConnectionCardAsync(ConnectionCard card, string toEmail)
    {
        var subject = $"[Forwarded] Connection Card: {card.Name}";
        var body = BuildAdminNotificationHtml(card);
        await SendAsync(toEmail, toEmail, subject, body);
    }

    public async Task SendVisitorConfirmationAsync(ConnectionCard card)
    {
        if (string.IsNullOrWhiteSpace(card.Email))
            return;

        var subject = "Thank you for connecting with Cedar Grove Baptist Church";
        var body = BuildVisitorConfirmationHtml(card);

        await SendAsync(card.Email, card.Name, subject, body);
    }

    private async Task SendAsync(string toAddress, string toName, string subject, string htmlBody)
    {
        var fromAddress = _config["Email:FromAddress"];
        var fromName = _config["Email:FromName"] ?? "Cedar Grove Baptist Church";
        var host = _config["Email:SmtpHost"];
        var port = int.TryParse(_config["Email:SmtpPort"], out var p) ? p : 587;
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];

        if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Email not configured — skipping send to {To}", toAddress);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(toName, toAddress));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", toAddress);
        }
    }

    internal static string BuildAdminNotificationHtml(ConnectionCard card)
    {
        var contactReasons = string.IsNullOrWhiteSpace(card.ContactReason) ? "None" : card.ContactReason;
        var prayerRequests = string.IsNullOrWhiteSpace(card.PrayerRequests) ? "None" : card.PrayerRequests;
        var address = string.IsNullOrWhiteSpace(card.Address) ? "—" : card.Address;
        var phone = string.IsNullOrWhiteSpace(card.Phone) ? "—" : card.Phone;
        var wantsContact = card.WantsContact ? "Yes" : "No";
        var submittedAt = card.SubmittedAt.ToLocalTime().ToString("f");

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;">
              <div style="background: #1a3a5c; padding: 20px; text-align: center;">
                <h1 style="color: #fff; margin: 0; font-size: 20px;">New Connection Card Submitted</h1>
              </div>
              <div style="padding: 24px; background: #f9f9f9;">
                <table style="width: 100%; border-collapse: collapse;">
                  <tr><td style="padding: 8px 0; font-weight: bold; width: 160px;">Name</td><td style="padding: 8px 0;">{Encode(card.Name)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Email</td><td style="padding: 8px 0;">{Encode(card.Email)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Phone</td><td style="padding: 8px 0;">{Encode(phone)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Address</td><td style="padding: 8px 0;">{Encode(address)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Visit Status</td><td style="padding: 8px 0;">{Encode(card.VisitStatus)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Wants Contact</td><td style="padding: 8px 0;">{wantsContact}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Preferred Contact</td><td style="padding: 8px 0;">{Encode(card.PreferredCommunication)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Reasons</td><td style="padding: 8px 0;">{Encode(contactReasons)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold; vertical-align: top;">Prayer Requests</td><td style="padding: 8px 0;">{Encode(prayerRequests)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Submitted</td><td style="padding: 8px 0;">{submittedAt}</td></tr>
                </table>
              </div>
              <div style="padding: 16px; text-align: center; background: #eee; font-size: 12px; color: #666;">
                Cedar Grove Baptist Church · <a href="https://cedargrovebaptist.church/admin/submissions/{card.Id}">View in Admin</a>
              </div>
            </body>
            </html>
            """;
    }

    internal static string BuildVisitorConfirmationHtml(ConnectionCard card)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;">
              <div style="background: #1a3a5c; padding: 20px; text-align: center;">
                <h1 style="color: #fff; margin: 0; font-size: 20px;">Thank You for Connecting!</h1>
              </div>
              <div style="padding: 24px;">
                <p>Dear {Encode(card.Name)},</p>
                <p>Thank you for filling out a connection card at Cedar Grove Baptist Church. We are so glad you joined us!</p>
                {(card.WantsContact ? "<p>Someone from our team will be reaching out to you soon.</p>" : "")}
                <p>In the meantime, feel free to visit our website at <a href="https://cedargrovebaptist.church">cedargrovebaptist.church</a> to learn more about our ministries and upcoming events.</p>
                <p>We look forward to seeing you again!</p>
                <p style="margin-top: 32px;">In Christ,<br><strong>Cedar Grove Baptist Church</strong></p>
              </div>
              <div style="padding: 16px; text-align: center; background: #eee; font-size: 12px; color: #666;">
                Cedar Grove Baptist Church
              </div>
            </body>
            </html>
            """;
    }

    private static string Encode(string? value) =>
        System.Net.WebUtility.HtmlEncode(value ?? "");
}
