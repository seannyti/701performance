using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PowersportsApi.Data;
using Microsoft.EntityFrameworkCore;

namespace PowersportsApi.Services;

public class EmailService
{
    private readonly PowersportsDbContext _context;
    private readonly ILogger<EmailService> _logger;

    public EmailService(PowersportsDbContext context, ILogger<EmailService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Load SMTP settings from the database
    /// </summary>
    private async Task<SmtpSettings?> GetSmtpSettingsAsync()
    {
        var keys = new[] { "smtp_host", "smtp_port", "smtp_username", "smtp_password", "smtp_from_email", "smtp_from_name", "smtp_use_ssl", "smtp_enabled" };
        var settings = await _context.SiteSettings
            .Where(s => keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        string host = settings.GetValueOrDefault("smtp_host", "");
        string fromEmail = settings.GetValueOrDefault("smtp_from_email", "");
        string username = settings.GetValueOrDefault("smtp_username", "");
        bool enabled = settings.GetValueOrDefault("smtp_enabled", "false") == "true";

        // Fall back to username as the from address if smtp_from_email is not set
        if (string.IsNullOrWhiteSpace(fromEmail))
            fromEmail = username;

        if (!enabled || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
            return null;

        return new SmtpSettings
        {
            Host = host,
            Port = int.TryParse(settings.GetValueOrDefault("smtp_port", "587"), out int port) ? port : 587,
            Username = settings.GetValueOrDefault("smtp_username", ""),
            Password = settings.GetValueOrDefault("smtp_password", ""),
            FromEmail = fromEmail,
            FromName = settings.GetValueOrDefault("smtp_from_name", "701 Performance Power"),
            UseSsl = settings.GetValueOrDefault("smtp_use_ssl", "true") == "true"
        };
    }

    /// <summary>
    /// Send an email using DB SMTP settings
    /// </summary>
    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            var smtp = await GetSmtpSettingsAsync();
            if (smtp == null)
            {
                _logger.LogWarning("Email not sent to {Email} - SMTP is not configured or disabled", toEmail);
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtp.FromName, smtp.FromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            // Port 465 uses implicit SSL (SslOnConnect); port 587 uses STARTTLS
            var secureOption = smtp.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : (smtp.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            await client.ConnectAsync(smtp.Host, smtp.Port, secureOption);

            if (!string.IsNullOrWhiteSpace(smtp.Username))
                await client.AuthenticateAsync(smtp.Username, smtp.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            return false;
        }
    }

    /// <summary>
    /// Send an email verification email
    /// </summary>
    public async Task<bool> SendVerificationEmailAsync(string toEmail, string toName, string token, string siteUrl)
    {
        var verifyUrl = $"{siteUrl.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(token)}";

        // Get site name for branding
        var siteName = await _context.SiteSettings
            .Where(s => s.Key == "site_name")
            .Select(s => s.Value)
            .FirstOrDefaultAsync() ?? "701 Performance Power";

        var subject = $"Verify your email — {siteName}";
        var html = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #333;'>
  <div style='background: linear-gradient(135deg, #ff6b35, #f7931e); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
    <h1 style='color: white; margin: 0; font-size: 24px;'>Welcome to {siteName}!</h1>
  </div>
  <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; border: 1px solid #eee;'>
    <p style='font-size: 16px;'>Hi {toName},</p>
    <p style='font-size: 16px;'>Thanks for signing up! Please verify your email address to activate your account.</p>
    <div style='text-align: center; margin: 30px 0;'>
      <a href='{verifyUrl}' style='background: #ff6b35; color: white; padding: 14px 32px; border-radius: 6px; text-decoration: none; font-size: 16px; font-weight: bold; display: inline-block;'>
        Verify Email Address
      </a>
    </div>
    <p style='font-size: 14px; color: #666;'>This link expires in 24 hours.</p>
    <p style='font-size: 14px; color: #666;'>If you didn't create an account, you can safely ignore this email.</p>
    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
    <p style='font-size: 12px; color: #999;'>If the button doesn't work, copy and paste this link into your browser:<br>
      <a href='{verifyUrl}' style='color: #ff6b35; word-break: break-all;'>{verifyUrl}</a>
    </p>
  </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, toName, subject, html);
    }

    /// <summary>
    /// Test the SMTP configuration by sending a test email
    /// </summary>
    public async Task<(bool Success, string Message)> SendTestEmailAsync(string toEmail)
    {
        try
        {
            var smtp = await GetSmtpSettingsAsync();
            if (smtp == null)
                return (false, "SMTP is not configured or is disabled. Please fill in all SMTP settings and enable SMTP first.");

            var sent = await SendEmailAsync(toEmail, toEmail, "SMTP Test — 701 Performance Power",
                "<h2>✅ SMTP is working!</h2><p>Your email settings are configured correctly.</p>");

            return sent
                ? (true, $"Test email sent successfully to {toEmail}")
                : (false, "Failed to send test email. Check logs for details.");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Send a contact form email to the site admin
    /// </summary>
    public async Task<(bool Success, string Message)> SendContactFormEmailAsync(string fromName, string fromEmail, string subject, string message)
    {
        try
        {
            var smtp = await GetSmtpSettingsAsync();
            if (smtp == null)
            {
                _logger.LogWarning("Contact form submission blocked - SMTP is disabled");
                return (false, "The contact form is currently unavailable. Please try again later or contact us directly.");
            }

            // Get the contact email to send to
            var contactEmail = await _context.SiteSettings
                .Where(s => s.Key == "contact_email")
                .Select(s => s.Value)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(contactEmail))
            {
                _logger.LogError("Contact email not configured in settings");
                return (false, "Contact form is not properly configured. Please contact the administrator.");
            }

            // Get site name for branding
            var siteName = await _context.SiteSettings
                .Where(s => s.Key == "site_name")
                .Select(s => s.Value)
                .FirstOrDefaultAsync() ?? "701 Performance Power";

            var emailSubject = string.IsNullOrWhiteSpace(subject) 
                ? $"New Contact Form Submission from {fromName}" 
                : $"Contact Form: {subject}";

            var html = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #333;'>
  <div style='background: linear-gradient(135deg, #ff6b35, #f7931e); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
    <h1 style='color: white; margin: 0; font-size: 24px;'>New Contact Form Submission</h1>
  </div>
  <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; border: 1px solid #eee;'>
    <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
      <tr>
        <td style='padding: 10px; border-bottom: 1px solid #ddd; font-weight: bold; width: 100px;'>From:</td>
        <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{fromName}</td>
      </tr>
      <tr>
        <td style='padding: 10px; border-bottom: 1px solid #ddd; font-weight: bold;'>Email:</td>
        <td style='padding: 10px; border-bottom: 1px solid #ddd;'><a href='mailto:{fromEmail}'>{fromEmail}</a></td>
      </tr>
      <tr>
        <td style='padding: 10px; border-bottom: 1px solid #ddd; font-weight: bold;'>Subject:</td>
        <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{(string.IsNullOrWhiteSpace(subject) ? "(No subject)" : subject)}</td>
      </tr>
      <tr>
        <td style='padding: 10px; font-weight: bold; vertical-align: top;'>Message:</td>
        <td style='padding: 10px;'></td>
      </tr>
    </table>
    <div style='background: white; padding: 20px; border-radius: 6px; border: 1px solid #ddd; white-space: pre-wrap;'>
{message}
    </div>
    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
    <p style='font-size: 12px; color: #999;'>This message was sent via the contact form on {siteName}.<br>
You can reply directly to <a href='mailto:{fromEmail}'>{fromEmail}</a></p>
  </div>
</body>
</html>";

            var sent = await SendEmailAsync(contactEmail, siteName, emailSubject, html);

            // Send confirmation email to the submitter
            if (sent)
            {
                var truncatedMessage = message.Length > 300
                    ? message.Substring(0, 300) + "..."
                    : message;

                var confirmationSubject = $"We received your message — {siteName}";
                var confirmationHtml = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #333;'>
  <div style='background: linear-gradient(135deg, #ff6b35, #f7931e); padding: 30px; border-radius: 8px 8px 0 0; text-align: center;'>
    <h1 style='color: white; margin: 0; font-size: 24px;'>Message Received!</h1>
  </div>
  <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; border: 1px solid #eee;'>
    <p style='font-size: 16px;'>Hi {fromName},</p>
    <p style='font-size: 16px;'>Thank you for contacting us. We've received your message and will get back to you as soon as possible.</p>
    <div style='background: white; padding: 20px; border-radius: 6px; border: 1px solid #ddd; margin: 20px 0;'>
      <p style='font-size: 14px; font-weight: bold; margin-bottom: 8px; color: #555;'>Your message:</p>
      <p style='font-size: 14px; color: #666; white-space: pre-wrap;'>{truncatedMessage}</p>
    </div>
    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
    <p style='font-size: 12px; color: #999;'>This is an automated confirmation from {siteName}. Please do not reply to this email.</p>
  </div>
</body>
</html>";

                // Log warning if confirmation fails but don't affect the main response
                var confirmationSent = await SendEmailAsync(fromEmail, fromName, confirmationSubject, confirmationHtml);
                if (!confirmationSent)
                {
                    _logger.LogWarning("Confirmation email to submitter {Email} could not be sent", fromEmail);
                }
            }

            return sent
                ? (true, "Message sent successfully! We'll get back to you soon.")
                : (false, "Failed to send message. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contact form email");
            return (false, "An error occurred while sending your message. Please try again later.");
        }
    }
}

public class SmtpSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "";
    public bool UseSsl { get; set; } = true;
}
