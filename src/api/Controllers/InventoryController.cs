using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformancePower.Api.Data;
using PerformancePower.Api.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PerformancePower.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _http;

    public InventoryController(AppDbContext db, IWebHostEnvironment env, IConfiguration config, IHttpClientFactory http)
    {
        _db = db;
        _env = env;
        _config = config;
        _http = http;
    }

    // Public — list all available vehicles with filters
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? type,
        [FromQuery] string? condition,
        [FromQuery] string? make,
        [FromQuery] string? model,
        [FromQuery] int? yearMin,
        [FromQuery] int? yearMax,
        [FromQuery] decimal? priceMin,
        [FromQuery] decimal? priceMax,
        [FromQuery] string? status,
        [FromQuery] string? sort = "newest",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var query = _db.Vehicles
            .Include(v => v.Images)
            .AsQueryable();

        // Authenticated requests are staff (no customer auth); public sees only available units
        var isStaff = User.Identity?.IsAuthenticated == true;
        if (!isStaff)
            query = query.Where(v => v.Status == "available");

        if (from.HasValue) query = query.Where(v => v.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(v => v.CreatedAt < to.Value.AddDays(1));

        if (!string.IsNullOrEmpty(search))
        {
            var words = search.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                var w = word;
                var stem = w.Length > 3 && w.EndsWith('s') ? w[..^1] : w;
                query = query.Where(v =>
                    v.Make.ToLower().Contains(w) ||
                    v.Model.ToLower().Contains(w) ||
                    v.StockNumber.ToLower().Contains(w) ||
                    v.Year.ToString().Contains(w) ||
                    v.Make.ToLower().Contains(stem) ||
                    v.Model.ToLower().Contains(stem) ||
                    (v.Vin != null && v.Vin.ToLower().Contains(w)));
            }
        }

        if (!string.IsNullOrEmpty(type)) query = query.Where(v => v.Type == type);
        if (!string.IsNullOrEmpty(condition)) query = query.Where(v => v.Condition == condition);
        if (!string.IsNullOrEmpty(make)) query = query.Where(v => v.Make == make);
        if (!string.IsNullOrEmpty(model)) query = query.Where(v => v.Model == model);
        if (!string.IsNullOrEmpty(status) && isStaff) query = query.Where(v => v.Status == status);
        if (yearMin.HasValue) query = query.Where(v => v.Year >= yearMin);
        if (yearMax.HasValue) query = query.Where(v => v.Year <= yearMax);
        if (priceMin.HasValue) query = query.Where(v => v.SalePrice >= priceMin);
        if (priceMax.HasValue) query = query.Where(v => v.SalePrice <= priceMax);

        query = sort switch
        {
            "price_asc" => query.OrderBy(v => v.SalePrice),
            "price_desc" => query.OrderByDescending(v => v.SalePrice),
            "year_desc" => query.OrderByDescending(v => v.Year),
            "year_asc" => query.OrderBy(v => v.Year),
            _ => query.OrderByDescending(v => v.CreatedAt)
        };

        var total = await query.CountAsync();
        var vehicles = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { total, page, pageSize, data = vehicles });
    }

    // Public — get vehicle detail
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vehicle = await _db.Vehicles
            .Include(v => v.Images)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null) return NotFound();
        return Ok(vehicle);
    }

    // Public — featured vehicles only
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var vehicles = await _db.Vehicles
            .Include(v => v.Images)
            .Where(v => v.IsFeatured && v.Status == "available")
            .OrderBy(v => v.CreatedAt)
            .Take(8)
            .ToListAsync();

        return Ok(vehicles);
    }

    // Portal — create vehicle
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest req)
    {
        var stockNumber = await GenerateStockNumberAsync();
        var vehicle = new Vehicle
        {
            StockNumber = stockNumber,
            Vin = req.Vin,
            Year = req.Year,
            Make = req.Make,
            Model = req.Model,
            Trim = req.Trim,
            Type = req.Type,
            Condition = req.Condition,
            Color = req.Color,
            Mileage = req.Mileage,
            Cost = req.Cost,
            Msrp = req.Msrp,
            SalePrice = req.SalePrice,
            Status = req.Status,
            Description = req.Description,
            Specs = req.Specs,
            IsFeatured = req.IsFeatured,
        };

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        await LogHistoryAsync(vehicle.Id, "created", null, vehicle);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    // Portal — update vehicle
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateVehicleRequest req)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();

        var old = new { vehicle.Status, vehicle.SalePrice, vehicle.IsFeatured };

        vehicle.Vin = req.Vin;
        vehicle.Year = req.Year;
        vehicle.Make = req.Make;
        vehicle.Model = req.Model;
        vehicle.Trim = req.Trim;
        vehicle.Type = req.Type;
        vehicle.Condition = req.Condition;
        vehicle.Color = req.Color;
        vehicle.Mileage = req.Mileage;
        vehicle.Cost = req.Cost;
        vehicle.Msrp = req.Msrp;
        vehicle.SalePrice = req.SalePrice;
        vehicle.Status = req.Status;
        vehicle.Description = req.Description;
        vehicle.Specs = req.Specs;
        vehicle.IsFeatured = req.IsFeatured;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await LogHistoryAsync(id, "updated", old, new { vehicle.Status, vehicle.SalePrice, vehicle.IsFeatured });

        return Ok(vehicle);
    }

    // Portal — delete vehicle
    [Authorize(Roles = "admin,superadmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();
        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Portal — toggle featured
    [Authorize]
    [HttpPut("{id}/featured")]
    public async Task<IActionResult> ToggleFeatured(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();
        vehicle.IsFeatured = !vehicle.IsFeatured;
        await _db.SaveChangesAsync();
        return Ok(new { vehicle.Id, vehicle.IsFeatured });
    }

    // Portal — upload images
    [Authorize]
    [HttpPost("{id}/images")]
    public async Task<IActionResult> UploadImages(int id, [FromForm] List<IFormFile> files)
    {
        var vehicle = await _db.Vehicles.Include(v => v.Images).FirstOrDefaultAsync(v => v.Id == id);
        if (vehicle == null) return NotFound();

        var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "vehicles", id.ToString());
        Directory.CreateDirectory(uploadPath);

        var addedImages = new List<VehicleImage>();
        var currentSortOrder = vehicle.Images.Count;

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            var fileName = $"{Guid.NewGuid()}.jpg";
            var thumbName = $"{Guid.NewGuid()}_thumb.jpg";
            var filePath = Path.Combine(uploadPath, fileName);
            var thumbPath = Path.Combine(uploadPath, thumbName);

            // Process with ImageSharp
            using var image = await Image.LoadAsync(file.OpenReadStream());
            image.Mutate(x => x.AutoOrient()); // respect EXIF orientation then strip

            // Display size — max 1200px wide
            var display = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(1200, 0),
                Mode = ResizeMode.Max
            }));
            await display.SaveAsJpegAsync(filePath);

            // Thumbnail — 400px wide
            var thumb = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(400, 0),
                Mode = ResizeMode.Max
            }));
            await thumb.SaveAsJpegAsync(thumbPath);

            var isPrimary = !vehicle.Images.Any() && currentSortOrder == 0;
            var vehicleImage = new VehicleImage
            {
                VehicleId = id,
                Url = $"/uploads/vehicles/{id}/{fileName}",
                ThumbnailUrl = $"/uploads/vehicles/{id}/{thumbName}",
                SortOrder = currentSortOrder++,
                IsPrimary = isPrimary
            };

            _db.VehicleImages.Add(vehicleImage);
            addedImages.Add(vehicleImage);
        }

        await _db.SaveChangesAsync();
        return Ok(addedImages);
    }

    // Portal — delete image
    [Authorize]
    [HttpDelete("{id}/images/{imageId}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var image = await _db.VehicleImages.FirstOrDefaultAsync(i => i.Id == imageId && i.VehicleId == id);
        if (image == null) return NotFound();

        // Delete files
        var webRoot = _env.WebRootPath;
        var filePath = Path.Combine(webRoot, image.Url.TrimStart('/'));
        var thumbPath = Path.Combine(webRoot, image.ThumbnailUrl.TrimStart('/'));
        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
        if (System.IO.File.Exists(thumbPath)) System.IO.File.Delete(thumbPath);

        _db.VehicleImages.Remove(image);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Portal — get vehicle history
    [Authorize]
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var history = await _db.VehicleHistories
            .Include(h => h.User)
            .Where(h => h.VehicleId == id)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        return Ok(history);
    }

    // Portal — upload spec sheet PDF
    [Authorize]
    [HttpPost("{id}/spec-sheet")]
    public async Task<IActionResult> UploadSpecSheet(int id, [FromForm] IFormFile file)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();
        if (file == null || file.Length == 0) return BadRequest(new { message = "No file provided." });
        if (!file.ContentType.Contains("pdf")) return BadRequest(new { message = "Only PDF files are accepted." });

        var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "vehicles", id.ToString(), "specs");
        Directory.CreateDirectory(uploadPath);

        // Delete old spec sheet file if exists
        if (!string.IsNullOrWhiteSpace(vehicle.SpecSheetUrl))
        {
            var oldPath = Path.Combine(_env.WebRootPath, vehicle.SpecSheetUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        }

        var fileName = $"specsheet_{Guid.NewGuid()}.pdf";
        var filePath = Path.Combine(uploadPath, fileName);
        using (var stream = System.IO.File.Create(filePath))
            await file.CopyToAsync(stream);

        vehicle.SpecSheetUrl = $"/uploads/vehicles/{id}/specs/{fileName}";
        vehicle.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { specSheetUrl = vehicle.SpecSheetUrl });
    }

    // Portal — parse spec sheet PDF with Claude AI
    [Authorize]
    [HttpPost("parse-spec")]
    public async Task<IActionResult> ParseSpec([FromForm] IFormFile file, [FromForm] string? modelHint)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "No file provided." });

        var apiKey = _config["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(503, new { message = "Anthropic API key not configured." });

        // Read PDF as base64
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var pdfBase64 = Convert.ToBase64String(ms.ToArray());

        var requestBody = new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 2048,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "document",
                            source = new
                            {
                                type = "base64",
                                media_type = "application/pdf",
                                data = pdfBase64
                            }
                        },
                        new
                        {
                            type = "text",
                            text = string.IsNullOrWhiteSpace(modelHint)
                                ? @"Extract vehicle information from this spec sheet and return ONLY a single JSON object — no markdown, no explanation.

The JSON must have two sections:

1. Core fields (omit any you cannot confidently identify):
   - year (number)
   - make (string)
   - model (string)
   - trim (string)
   - type (one of: atv, utv, motorcycle, dirtbike, snowmobile, other)
   - color (string, primary/standard color if listed)
   - msrp (number, no currency symbol)
   - description (string, 1-2 sentence summary of the vehicle)

2. specs (array of { ""label"": string, ""value"": string } objects):
   Include EVERY additional technical specification found on the sheet — things like engine displacement, cylinders, horsepower, torque, transmission, drive system, suspension, ground clearance, wheelbase, dimensions, weight, tow capacity, payload, fuel capacity, tire size, brakes, etc. Capture all of them, using the exact label names from the document.

Example format:
{
  ""year"": 2024,
  ""make"": ""Vitacci"",
  ""model"": ""Ranger 200"",
  ""type"": ""utv"",
  ""msrp"": 4999,
  ""description"": ""200cc utility UTV with 4WD and automatic transmission."",
  ""specs"": [
    { ""label"": ""Engine"", ""value"": ""200cc Single Cylinder 4-Stroke"" },
    { ""label"": ""Transmission"", ""value"": ""CVT H/L/N/R/P"" },
    { ""label"": ""Drive"", ""value"": ""4WD"" },
    { ""label"": ""Tire Size (Front)"", ""value"": ""23x7-10"" }
  ]
}"
                                : $@"This document is a multi-model catalog. Extract information ONLY for the model: ""{modelHint}"". Ignore all other models. Return ONLY a single JSON object — no markdown, no explanation.

The JSON must have two sections:

1. Core fields (omit any you cannot confidently identify):
   - year (number)
   - make (string)
   - model (string, use ""{modelHint}"" as the base)
   - trim (string)
   - type (one of: atv, utv, motorcycle, dirtbike, snowmobile, other)
   - color (string, primary/standard color if listed)
   - msrp (number, no currency symbol)
   - description (string, 1-2 sentence summary of the vehicle)

2. specs (array of {{ ""label"": string, ""value"": string }} objects):
   Include EVERY additional technical specification found for this specific model — engine displacement, cylinders, horsepower, torque, transmission, drive system, suspension, ground clearance, wheelbase, dimensions, weight, tow capacity, payload, fuel capacity, tire size, brakes, etc. Capture all of them using the exact label names from the document.

Example format:
{{
  ""year"": 2025,
  ""make"": ""Vitacci"",
  ""model"": ""{modelHint}"",
  ""type"": ""atv"",
  ""msrp"": 6999,
  ""description"": ""Powerful ATV with liquid-cooled engine and 4WD."",
  ""specs"": [
    {{ ""label"": ""Engine"", ""value"": ""580cc Single Cylinder 4-Stroke"" }},
    {{ ""label"": ""Transmission"", ""value"": ""CVT"" }},
    {{ ""label"": ""Drive"", ""value"": ""2WD/4WD"" }}
  ]
}}"
                        }
                    }
                }
            }
        };

        var client = _http.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return StatusCode(502, new { message = "Claude API error.", detail = err });
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        // Strip any markdown code fences Claude might add
        text = text.Trim();
        if (text.StartsWith("```")) text = text.Split('\n', 2)[1];
        if (text.EndsWith("```")) text = text[..text.LastIndexOf("```")];

        try
        {
            using var parsed = JsonDocument.Parse(text.Trim());
            return Ok(parsed.RootElement.Clone());
        }
        catch
        {
            return Ok(new { });
        }
    }

    private async Task<string> GenerateStockNumberAsync()
    {
        var year = DateTime.UtcNow.Year.ToString()[2..];
        var count = await _db.Vehicles.CountAsync() + 1;
        return $"PP{year}{count:D4}";
    }

    private async Task LogHistoryAsync(int vehicleId, string action, object? oldValues, object? newValues)
    {
        _db.VehicleHistories.Add(new VehicleHistory
        {
            VehicleId = vehicleId,
            Action = action,
            OldValues = oldValues != null ? System.Text.Json.JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? System.Text.Json.JsonSerializer.Serialize(newValues) : null
        });
        await _db.SaveChangesAsync();
    }
}

public record CreateVehicleRequest(
    string? Vin,
    int Year,
    string Make,
    string Model,
    string? Trim,
    string Type,
    string Condition,
    string? Color,
    int? Mileage,
    decimal Cost,
    decimal Msrp,
    decimal SalePrice,
    string Status,
    string? Description,
    string? Specs,
    bool IsFeatured = false
);
