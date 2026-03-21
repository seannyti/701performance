using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using mperformancepower.Api.Data;
using mperformancepower.Api.DTOs.Inquiry;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Services;

public class MailService(AppDbContext db, ILogger<MailService> logger) : IMailService
{
    public async Task SendInquiryConfirmationAsync(InquiryDto inquiry)
    {
        var cfg = await LoadEmailConfigAsync();
        var subject = "We received your inquiry — Minot Performance Powersports";
        var body = $"Hi {inquiry.Name},\n\nThank you for reaching out! We'll be in touch shortly.\n\nMinot Performance Powersports";
        await SendAsync(cfg, inquiry.Email, inquiry.Name, subject, body);
    }

    public async Task SendAdminNotificationAsync(InquiryDto inquiry)
    {
        // Load config once — do NOT call DB again after this point
        var cfg = await LoadEmailConfigAsync();

        var recipients = cfg.InquiryRecipients;
        if (recipients.Count == 0 && !string.IsNullOrWhiteSpace(cfg.SmtpUser))
            recipients = [cfg.SmtpUser];

        if (recipients.Count == 0) return;

        var subject = $"New Inquiry from {inquiry.Name}";
        var vehicle = string.IsNullOrWhiteSpace(inquiry.VehicleName) ? "General" : inquiry.VehicleName;
        var htmlBody = $@"<html><body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px'>
<div style='max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.1)'>
  <div style='background:#e63946;padding:24px 32px'>
    <h2 style='color:#fff;margin:0;font-size:1.3rem'>New Contact Form Submission</h2>
  </div>
  <div style='padding:32px'>
    <table style='width:100%;border-collapse:collapse;font-size:0.95rem'>
      <tr><td style='padding:10px 0;border-bottom:1px solid #eee;color:#555;width:120px'><strong>From:</strong></td><td style='padding:10px 0;border-bottom:1px solid #eee'>{inquiry.Name}</td></tr>
      <tr><td style='padding:10px 0;border-bottom:1px solid #eee;color:#555'><strong>Email:</strong></td><td style='padding:10px 0;border-bottom:1px solid #eee'><a href='mailto:{inquiry.Email}' style='color:#e63946'>{inquiry.Email}</a></td></tr>
      <tr><td style='padding:10px 0;border-bottom:1px solid #eee;color:#555'><strong>Phone:</strong></td><td style='padding:10px 0;border-bottom:1px solid #eee'>{inquiry.Phone}</td></tr>
      <tr><td style='padding:10px 0;border-bottom:1px solid #eee;color:#555'><strong>Vehicle:</strong></td><td style='padding:10px 0;border-bottom:1px solid #eee'>{vehicle}</td></tr>
    </table>
    <div style='margin-top:20px'>
      <p style='color:#555;margin-bottom:8px'><strong>Message:</strong></p>
      <div style='background:#f9f9f9;border:1px solid #eee;border-radius:6px;padding:16px;font-size:0.9rem;color:#333;line-height:1.6'>{inquiry.Message}</div>
    </div>
    <p style='margin-top:24px;font-size:0.8rem;color:#999'>This message was sent via the contact form on Minot Performance Powersports.<br>You can reply directly to <a href='mailto:{inquiry.Email}' style='color:#e63946'>{inquiry.Email}</a></p>
  </div>
</div></body></html>";

        foreach (var recipient in recipients)
            await SendHtmlAsync(cfg, recipient, "Admin", subject, htmlBody);
    }

    public async Task SendEmailVerificationAsync(string toEmail, string name, string token, string publicBaseUrl)
    {
        var cfg = await LoadEmailConfigAsync();
        var link = $"{publicBaseUrl.TrimEnd('/')}/verify-email?token={token}";
        var subject = "Verify your email — Minot Performance Powersports";
        var body = $"Hi {name},\n\nPlease verify your email address by clicking the link below:\n\n{link}\n\nThis link expires in 24 hours.\n\nMinot Performance Powersports";
        await SendAsync(cfg, toEmail, name, subject, body);
    }

    public async Task SendTempPasswordAsync(string toEmail, string tempPassword)
    {
        var cfg = await LoadEmailConfigAsync();
        var subject = "Your temporary password — Minot Performance Powersports";
        var body = $"A temporary password has been set for your account:\n\n{tempPassword}\n\nPlease log in and change your password immediately.\n\nMinot Performance Powersports";
        await SendAsync(cfg, toEmail, toEmail, subject, body);
    }

    private async Task SendAsync(EmailConfig cfg, string toAddress, string toName, string subject, string body)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cfg.SmtpHost))
            {
                logger.LogWarning("SMTP is not configured — email to {Address} skipped", toAddress);
                return;
            }

            var fromAddress = cfg.ReplyFrom.Length > 0 ? cfg.ReplyFrom : cfg.SmtpUser;
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Minot Performance Powersports", fromAddress));
            message.To.Add(new MailboxAddress(toName, toAddress));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            var socketOptions = cfg.SmtpSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
            await client.ConnectAsync(cfg.SmtpHost, cfg.SmtpPort, socketOptions);
            if (!string.IsNullOrWhiteSpace(cfg.SmtpUser))
                await client.AuthenticateAsync(cfg.SmtpUser, cfg.SmtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Address}", toAddress);
        }
    }

    private async Task SendHtmlAsync(EmailConfig cfg, string toAddress, string toName, string subject, string htmlBody)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cfg.SmtpHost)) return;

            var fromAddress = cfg.ReplyFrom.Length > 0 ? cfg.ReplyFrom : cfg.SmtpUser;
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Minot Performance Powersports", fromAddress));
            message.To.Add(new MailboxAddress(toName, toAddress));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            var socketOptions = cfg.SmtpSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
            await client.ConnectAsync(cfg.SmtpHost, cfg.SmtpPort, socketOptions);
            if (!string.IsNullOrWhiteSpace(cfg.SmtpUser))
                await client.AuthenticateAsync(cfg.SmtpUser, cfg.SmtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            logger.LogInformation("HTML email sent successfully to {Address} | Subject: {Subject}", toAddress, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send HTML email to {Address} | {Message}", toAddress, ex.Message);
        }
    }

    private async Task<EmailConfig> LoadEmailConfigAsync()
    {
        var setting = await db.SiteSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Section == "email");

        if (setting is null) return new EmailConfig();

        try
        {
            var doc = JsonDocument.Parse(setting.DataJson);
            var root = doc.RootElement;

            var inquiryRecipients = new List<string>();
            if (root.TryGetProperty("inquiryRecipients", out var arr))
                inquiryRecipients = arr.EnumerateArray()
                    .Select(e => e.GetString() ?? "")
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

            return new EmailConfig
            {
                SmtpHost = root.TryGetProperty("smtpHost", out var h) ? h.GetString() ?? "" : "",
                SmtpPort = root.TryGetProperty("smtpPort", out var p) && p.TryGetInt32(out var port) ? port : 587,
                SmtpUser = root.TryGetProperty("smtpUser", out var u) ? u.GetString() ?? "" : "",
                SmtpPass = root.TryGetProperty("smtpPass", out var pw) ? pw.GetString() ?? "" : "",
                SmtpSsl  = root.TryGetProperty("smtpSsl",  out var ssl) && ssl.ValueKind == JsonValueKind.True,
                ReplyFrom = root.TryGetProperty("replyFrom", out var rf) ? rf.GetString() ?? "" : "",
                InquiryRecipients = inquiryRecipients,
            };
        }
        catch
        {
            return new EmailConfig();
        }
    }

    private sealed class EmailConfig
    {
        public string SmtpHost  { get; init; } = "";
        public int    SmtpPort  { get; init; } = 587;
        public string SmtpUser  { get; init; } = "";
        public string SmtpPass  { get; init; } = "";
        public bool   SmtpSsl   { get; init; } = false;
        public string ReplyFrom { get; init; } = "";
        public List<string> InquiryRecipients { get; init; } = [];
    }
}
