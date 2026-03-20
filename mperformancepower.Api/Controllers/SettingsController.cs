using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Data;
using mperformancepower.Api.Models;
using System.Text.Json;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController(AppDbContext db) : ControllerBase
{
    // Public: any page can read settings
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await db.SiteSettings.ToListAsync();
        var result = settings.ToDictionary(
            s => s.Section,
            s => JsonSerializer.Deserialize<JsonElement>(s.DataJson)
        );
        return Ok(result);
    }

    [HttpGet("{section}")]
    public async Task<IActionResult> Get(string section)
    {
        var setting = await db.SiteSettings.FindAsync(section);
        if (setting is null) return Ok(new { });
        return Content(setting.DataJson, "application/json");
    }

    // Admin only: update a section
    [HttpPut("{section}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string section, [FromBody] JsonElement data)
    {
        var setting = await db.SiteSettings.FindAsync(section);
        if (setting is null)
        {
            setting = new SiteSetting { Section = section };
            db.SiteSettings.Add(setting);
        }
        setting.DataJson = JsonSerializer.Serialize(data);
        setting.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok();
    }
}
