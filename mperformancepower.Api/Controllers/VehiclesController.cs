using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mperformancepower.Api.DTOs.Vehicle;
using mperformancepower.Api.Enums;
using mperformancepower.Api.Services.Interfaces;
using SixLabors.ImageSharp;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? condition = null,
        [FromQuery] bool? featured = null,
        [FromQuery] string? search = null)
    {
        VehicleCondition? parsedCondition = null;
        if (!string.IsNullOrWhiteSpace(condition) &&
            Enum.TryParse<VehicleCondition>(condition, ignoreCase: true, out var c))
            parsedCondition = c;

        var result = await vehicleService.GetVehiclesAsync(page, pageSize, categoryId, parsedCondition, featured, search);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var vehicle = await vehicleService.GetVehicleAsync(id);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
    {
        var vehicle = await vehicleService.CreateVehicleAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleDto dto)
    {
        var vehicle = await vehicleService.UpdateVehicleAsync(id, dto);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await vehicleService.DeleteVehicleAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/images"), Authorize(Roles = "Admin")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB total request
    public async Task<IActionResult> UploadImages(int id, IFormFileCollection files)
    {
        if (files.Count == 0)
            return BadRequest(new { message = "No files provided." });

        const long maxFileSize = 15 * 1024 * 1024; // 15 MB per file
        foreach (var file in files)
        {
            if (file.Length > maxFileSize)
                return BadRequest(new { message = $"File '{file.FileName}' exceeds the 15 MB limit." });

            var info = await Image.IdentifyAsync(file.OpenReadStream());
            if (info is null)
                return BadRequest(new { message = $"File '{file.FileName}' is not a valid image." });
        }

        var images = await vehicleService.UploadImagesAsync(id, files);
        return Ok(images);
    }
}
