using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/hero")]
public class HeroController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var hero = await db.HeroSettings.FirstOrDefaultAsync();
        return hero is null ? NotFound() : Ok(hero);
    }

    [HttpPut, Authorize]
    public async Task<IActionResult> Update([FromBody] UpdateHeroRequest req)
    {
        var hero = await db.HeroSettings.FirstOrDefaultAsync();
        if (hero is null) return NotFound();

        hero.HeroType = req.HeroType;
        hero.YoutubeUrl = req.YoutubeUrl;
        hero.YoutubeStartTime = req.YoutubeStartTime;
        hero.OverlayOpacity = req.OverlayOpacity;
        hero.VideoPath = req.VideoPath;
        hero.ImagePath = req.ImagePath;
        hero.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(hero);
    }

    [HttpPost("upload"), Authorize]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var hero = await db.HeroSettings.FirstOrDefaultAsync();
        if (hero is null) return NotFound();

        var heroDir = Path.Combine(env.WebRootPath, "uploads", "hero");
        Directory.CreateDirectory(heroDir);

        string relativePath;

        if (file.ContentType.StartsWith("video/"))
        {
            var fileName = $"{Guid.NewGuid()}.mp4";
            var fullPath = Path.Combine(heroDir, fileName);
            using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream);
            relativePath = $"hero/{fileName}";
            hero.VideoPath = relativePath;
        }
        else
        {
            var fileName = $"{Guid.NewGuid()}.webp";
            var fullPath = Path.Combine(heroDir, fileName);
            using var image = await Image.LoadAsync(file.OpenReadStream());
            if (image.Width > 1920) image.Mutate(x => x.Resize(1920, 0));
            await image.SaveAsWebpAsync(fullPath, new WebpEncoder { Quality = 85 });
            relativePath = $"hero/{fileName}";
            hero.ImagePath = relativePath;
        }

        hero.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { path = relativePath });
    }
}

public record UpdateHeroRequest(
    string HeroType,
    string? YoutubeUrl,
    int YoutubeStartTime,
    double OverlayOpacity,
    string? VideoPath,
    string? ImagePath);
