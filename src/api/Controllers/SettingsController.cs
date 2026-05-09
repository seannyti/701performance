using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PerformancePower.Api.Data;
using PerformancePower.Api.Helpers;
using PerformancePower.Api.Models;
using PerformancePower.Api.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PerformancePower.Api.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly EmailService _email;
    private readonly IMemoryCache _cache;

    private const string SettingsCacheKey = "site_settings";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    // Keys never returned to the public (unauthenticated) endpoint
    private static readonly HashSet<string> _sensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "smtp_password"
    };

    public SettingsController(AppDbContext db, IWebHostEnvironment env, EmailService email, IMemoryCache cache)
    {
        _db    = db;
        _env   = env;
        _email = email;
        _cache = cache;
    }

    private async Task<List<SiteSetting>> GetCachedSettingsAsync()
    {
        if (!_cache.TryGetValue(SettingsCacheKey, out List<SiteSetting>? settings) || settings == null)
        {
            settings = await _db.SiteSettings.ToListAsync();
            _cache.Set(SettingsCacheKey, settings, CacheDuration);
        }
        return settings;
    }

    private void InvalidateCache() => _cache.Remove(SettingsCacheKey);

    // Public read — no auth required, sensitive keys excluded
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await GetCachedSettingsAsync();
        var dict = settings
            .Where(s => !_sensitiveKeys.Contains(s.Key))
            .ToDictionary(s => s.Key, s => s.Value);
        return Ok(dict);
    }

    // Authenticated read — returns everything including sensitive keys
    [Authorize(Roles = "admin,superadmin")]
    [HttpGet("secure")]
    public async Task<IActionResult> GetAllSecure()
    {
        var settings = await _db.SiteSettings.ToListAsync(); // always fresh for admin
        var dict = settings.ToDictionary(s => s.Key, s => s.Value);
        return Ok(dict);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.To))
            return BadRequest(new { message = "Recipient email is required." });

        try
        {
            await _email.SendAsync(req.To, "PerformancePower — SMTP Test", """
                <h2>✅ SMTP Test Successful</h2>
                <p>Your email configuration is working correctly.</p>
                <p style="color:#888;font-size:12px">Sent from PerformancePower DMS</p>
                """);
            return Ok(new { message = "Test email sent successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Portal write — admin only
    [Authorize(Roles = "admin,superadmin")]
    [HttpPut]
    public async Task<IActionResult> BulkUpdate([FromBody] Dictionary<string, string> updates)
    {
        var userId = User.GetUserId();

        foreach (var (key, value) in updates)
        {
            var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null)
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedByUserId = userId;
            }
            else
            {
                _db.SiteSettings.Add(new SiteSetting
                {
                    Key = key,
                    Value = value,
                    UpdatedByUserId = userId
                });
            }
        }

        await _db.SaveChangesAsync();
        InvalidateCache();
        return Ok(new { message = "Settings saved" });
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPut("{key}")]
    public async Task<IActionResult> UpdateByKey(string key, [FromBody] UpdateSettingRequest req)
    {
        var userId = User.GetUserId();
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            _db.SiteSettings.Add(new SiteSetting
            {
                Key = key,
                Value = req.Value,
                Type = req.Type ?? "string",
                UpdatedByUserId = userId
            });
        }
        else
        {
            setting.Value = req.Value;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedByUserId = userId;
        }

        await _db.SaveChangesAsync();
        InvalidateCache();
        return Ok(new { key, value = req.Value });
    }

    // File uploads for settings assets
    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("upload/image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file");
        var url = await SaveImage(file, "settings");
        return Ok(new { url });
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("upload/logo")]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file");
        var url = await SaveImage(file, "settings/logos");
        return Ok(new { url });
    }

    private static readonly HashSet<string> _allowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm", ".mov"
    };

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("upload/video")]
    public async Task<IActionResult> UploadVideo(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file");
        if (file.Length > 200 * 1024 * 1024) return BadRequest("Video must be under 200 MB");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedVideoExtensions.Contains(ext))
            return BadRequest("Only .mp4, .webm, and .mov files are allowed");

        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "settings", "videos");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        var url = $"/uploads/settings/videos/{fileName}";
        return Ok(new { url });
    }

    private async Task<string> SaveImage(IFormFile file, string subFolder)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", subFolder);
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}.jpg";
        var filePath = Path.Combine(uploadsPath, fileName);

        using var image = await Image.LoadAsync(file.OpenReadStream());
        image.Mutate(x => x
            .AutoOrient()
            .Resize(new ResizeOptions { Size = new SixLabors.ImageSharp.Size(1920, 1080), Mode = ResizeMode.Max }));

        await image.SaveAsJpegAsync(filePath, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 85 });

        return $"/uploads/{subFolder}/{fileName}";
    }
}

public record UpdateSettingRequest(string Value, string? Type);
public record TestEmailRequest(string To);
