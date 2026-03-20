using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImagesController(IVehicleService vehicleService) : ControllerBase
{
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await vehicleService.DeleteImageAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpPut("{id:int}/set-primary")]
    public async Task<IActionResult> SetPrimary(int id)
    {
        var success = await vehicleService.SetPrimaryImageAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<ImageReorderItem> items)
    {
        var reorders = items.Select(i => (i.Id, i.Order)).ToList();
        await vehicleService.ReorderImagesAsync(reorders);
        return NoContent();
    }
}

public record ImageReorderItem(int Id, int Order);
