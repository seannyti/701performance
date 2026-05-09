using Microsoft.AspNetCore.Mvc;
using PerformancePower.Api.Data;
using PerformancePower.Api.Services;

namespace PerformancePower.Api.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;
    private readonly ILogger<ContactController> _logger;

    public ContactController(AppDbContext db, EmailService email, ILogger<ContactController> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] ContactFormRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest(new { message = "Name, email, and message are required." });

        if (req.Name.Length > 100)
            return BadRequest(new { message = "Name must be 100 characters or fewer." });
        if (req.Message.Length > 5000)
            return BadRequest(new { message = "Message must be 5000 characters or fewer." });
        if (req.Subject != null && req.Subject.Length > 200)
            return BadRequest(new { message = "Subject must be 200 characters or fewer." });
        if (req.Phone != null && req.Phone.Length > 30)
            return BadRequest(new { message = "Phone must be 30 characters or fewer." });

        // Prevent email header injection — strip newlines from the email field
        if (req.Email.Contains('\r') || req.Email.Contains('\n') || req.Email.Length > 254)
            return BadRequest(new { message = "Invalid email address." });
        if (!System.Net.Mail.MailAddress.TryCreate(req.Email, out _))
            return BadRequest(new { message = "Invalid email address." });

        var recipients = await _email.GetRecipientsAsync("notification_inquiry_emails");
        var siteName = await _email.GetSiteNameAsync() ?? "PerformancePower";
        var footer = await _email.GetEmailFooterAsync();
        var footerHtml = string.IsNullOrWhiteSpace(footer) ? "" : $"<hr/><p style='color:#888;font-size:12px'>{System.Net.WebUtility.HtmlEncode(footer)}</p>";

        // Notify admin(s)
        if (recipients.Any())
        {
            try
            {
                var adminHtml = $"""
                    <h2>New Contact Form Submission</h2>
                    <table>
                      <tr><td><strong>Name:</strong></td><td>{System.Net.WebUtility.HtmlEncode(req.Name)}</td></tr>
                      <tr><td><strong>Email:</strong></td><td>{System.Net.WebUtility.HtmlEncode(req.Email)}</td></tr>
                      {(string.IsNullOrWhiteSpace(req.Phone) ? "" : $"<tr><td><strong>Phone:</strong></td><td>{System.Net.WebUtility.HtmlEncode(req.Phone)}</td></tr>")}
                      {(string.IsNullOrWhiteSpace(req.Subject) ? "" : $"<tr><td><strong>Subject:</strong></td><td>{System.Net.WebUtility.HtmlEncode(req.Subject)}</td></tr>")}
                    </table>
                    <hr/>
                    <p><strong>Message:</strong></p>
                    <p>{System.Net.WebUtility.HtmlEncode(req.Message).Replace("\n", "<br/>")}</p>
                    {footerHtml}
                    """;

                await _email.SendAsync(recipients, $"Website Contact: {req.Subject ?? "General Inquiry"}", adminHtml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send admin contact notification");
            }
        }

        // Send confirmation to the submitter
        try
        {
            var confirmHtml = $"""
                <div style="font-family:sans-serif;max-width:600px;margin:0 auto;color:#333">
                  <h2 style="color:#e53935">{System.Net.WebUtility.HtmlEncode(siteName)}</h2>
                  <p>Hi {System.Net.WebUtility.HtmlEncode(req.Name)},</p>
                  <p>Thanks for reaching out! We've received your message and will get back to you as soon as possible.</p>
                  <div style="background:#f5f5f5;border-left:4px solid #e53935;padding:12px 16px;margin:20px 0;border-radius:4px">
                    <p style="margin:0 0 8px;font-size:13px;color:#888;text-transform:uppercase;letter-spacing:1px">Your message</p>
                    <p style="margin:0;white-space:pre-wrap">{System.Net.WebUtility.HtmlEncode(req.Message)}</p>
                  </div>
                  <p>If you have any additional questions, feel free to reply to this email.</p>
                  <p>— The {System.Net.WebUtility.HtmlEncode(siteName)} Team</p>
                  {footerHtml}
                </div>
                """;

            await _email.SendAsync(req.Email, $"We received your message — {siteName}", confirmHtml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contact confirmation to {Email}", req.Email);
        }

        return Ok(new { message = "Message received. We'll be in touch soon!" });
    }
}

public record ContactFormRequest(string Name, string Email, string? Phone, string? Subject, string Message);
