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
        var subject = "We received your inquiry — Minot Performance Powersports";
        var body = $"Hi {inquiry.Name},\n\nThank you for reaching out! We'll be in touch shortly.\n\nMinot Performance Powersports";
        await SendAsync(inquiry.Email, inquiry.Name, subject, body);
    }

    public async Task SendAdminNotificationAsync(InquiryDto inquiry)
    {
        var cfg = await LoadEmailConfigAsync();
        var from = cfg.ReplyFrom.Length > 0 ? cfg.ReplyFrom : cfg.SmtpUser;
        var subject = $"New Inquiry from {inquiry.Name}";
        var body = $"Name: {inquiry.Name}\nEmail: {inquiry.Email}\nPhone: {inquiry.Phone}\n\n{inquiry.Message}";
        await SendAsync(from, "Admin", subject, body);
    }

    public async Task SendEmailVerificationAsync(string toEmail, string name, string token, string publicBaseUrl)
    {
        var link = $"{publicBaseUrl.TrimEnd('/')}/verify-email?token={token}";
        var subject = "Verify your email — Minot Performance Powersports";
        var body = $"Hi {name},\n\nPlease verify your email address by clicking the link below:\n\n{link}\n\nThis link expires in 24 hours.\n\nMinot Performance Powersports";
        await SendAsync(toEmail, name, subject, body);
    }

    public async Task SendTempPasswordAsync(string toEmail, string tempPassword)
    {
        var subject = "Your temporary password — Minot Performance Powersports";
        var body = $"A temporary password has been set for your account:\n\n{tempPassword}\n\nPlease log in and change your password immediately.\n\nMinot Performance Powersports";
        await SendAsync(toEmail, toEmail, subject, body);
    }

    private async Task SendAsync(string toAddress, string toName, string subject, string body)
    {
        try
        {
            var cfg = await LoadEmailConfigAsync();

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

    private async Task<EmailConfig> LoadEmailConfigAsync()
    {
        var setting = await db.SiteSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Section == "email");

        if (setting is null) return new EmailConfig();

        try
        {
            var doc = JsonDocument.Parse(setting.DataJson);
            var root = doc.RootElement;
            return new EmailConfig
            {
                SmtpHost = root.TryGetProperty("smtpHost", out var h) ? h.GetString() ?? "" : "",
                SmtpPort = root.TryGetProperty("smtpPort", out var p) && p.TryGetInt32(out var port) ? port : 587,
                SmtpUser = root.TryGetProperty("smtpUser", out var u) ? u.GetString() ?? "" : "",
                SmtpPass = root.TryGetProperty("smtpPass", out var pw) ? pw.GetString() ?? "" : "",
                SmtpSsl  = root.TryGetProperty("smtpSsl",  out var ssl) && ssl.ValueKind == JsonValueKind.True,
                ReplyFrom = root.TryGetProperty("replyFrom", out var rf) ? rf.GetString() ?? "" : "",
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
    }
}
