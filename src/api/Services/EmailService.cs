using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using PerformancePower.Api.Data;

namespace PerformancePower.Api.Services;

public class EmailService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(AppDbContext db, IConfiguration config, ILogger<EmailService> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task<SmtpConfig> GetSmtpConfigAsync()
    {
        var settings = await _db.SiteSettings
            .Where(s => s.Key.StartsWith("smtp_"))
            .ToListAsync();

        string? Get(string key) => settings.FirstOrDefault(s => s.Key == key)?.Value;

        var host    = Get("smtp_host")     ?? _config["Smtp:Host"];
        var portStr = Get("smtp_port")     ?? _config["Smtp:Port"] ?? "587";
        var user    = Get("smtp_username") ?? _config["Smtp:Username"];
        var pass    = Get("smtp_password") ?? _config["Smtp:Password"];
        var from    = Get("smtp_from")     ?? _config["Smtp:From"] ?? user;
        var sslStr  = Get("smtp_ssl")      ?? _config["Smtp:Ssl"] ?? "false";

        int.TryParse(portStr, out var port);
        var ssl = string.Equals(sslStr, "true", StringComparison.OrdinalIgnoreCase);

        return new SmtpConfig(host, port == 0 ? 587 : port, user, pass, from, ssl);
    }

    private async Task<string> BuildCanSpamFooterAsync()
    {
        var settings = await _db.SiteSettings
            .Where(s => s.Key == "contact_address" || s.Key == "site_name" || s.Key == "seo_title")
            .ToListAsync();

        string? Get(string key) => settings.FirstOrDefault(s => s.Key == key)?.Value;
        var address  = Get("contact_address");
        var siteName = Get("site_name") ?? Get("seo_title") ?? "PerformancePower Powersports";

        if (string.IsNullOrWhiteSpace(address))
            address = "[Dealership address not configured — please set contact_address in Settings]";

        return $"""
            <div style="margin-top:32px;padding-top:16px;border-top:1px solid #e0e0e0;font-size:11px;color:#888;text-align:center;font-family:sans-serif;">
              <p style="margin:4px 0;">{System.Net.WebUtility.HtmlEncode(siteName)}</p>
              <p style="margin:4px 0;">{System.Net.WebUtility.HtmlEncode(address)}</p>
              <p style="margin:8px 0;">This is a transactional email sent in response to a contact form submission.</p>
            </div>
            """;
    }

    public async Task SendAsync(IEnumerable<string> toAddresses, string subject, string htmlBody)
    {
        var cfg = await GetSmtpConfigAsync();

        if (string.IsNullOrWhiteSpace(cfg.Host))
            throw new InvalidOperationException("SMTP host is not configured.");

        var footer = await BuildCanSpamFooterAsync();
        var fullBody = htmlBody + footer;

        var message = new MimeMessage();
        var fromAddress = cfg.From ?? cfg.Username ?? throw new InvalidOperationException("SMTP from address is not configured.");
        message.From.Add(MailboxAddress.Parse(fromAddress));

        foreach (var addr in toAddresses.Where(a => !string.IsNullOrWhiteSpace(a)))
            message.To.Add(MailboxAddress.Parse(addr.Trim()));

        if (!message.To.Any())
            throw new InvalidOperationException("No valid recipients.");

        message.Subject = subject;
        message.Body = new TextPart("html") { Text = fullBody };

        var socketOptions = cfg.UseSsl
            ? MailKit.Security.SecureSocketOptions.SslOnConnect
            : MailKit.Security.SecureSocketOptions.StartTls;

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(cfg.Host, cfg.Port, socketOptions);
        if (!string.IsNullOrWhiteSpace(cfg.Username))
            await smtp.AuthenticateAsync(cfg.Username, cfg.Password ?? "");
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }

    // Convenience overload — single recipient
    public Task SendAsync(string toAddress, string subject, string htmlBody)
        => SendAsync(new[] { toAddress }, subject, htmlBody);

    public async Task<List<string>> GetRecipientsAsync(string settingKey)
    {
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == settingKey);
        if (string.IsNullOrWhiteSpace(setting?.Value)) return new List<string>();
        return setting.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    public async Task<string?> GetSiteNameAsync()
    {
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "site_name");
        return setting?.Value;
    }

    public async Task<string> GetEmailFooterAsync()
    {
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "email_footer");
        return setting?.Value ?? string.Empty;
    }
}

public record SmtpConfig(string? Host, int Port, string? Username, string? Password, string? From, bool UseSsl);
