using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mperformancepower.Api.DTOs.Inquiry;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InquiriesController(IInquiryService inquiryService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInquiryDto dto)
    {
        var inquiry = await inquiryService.CreateInquiryAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = inquiry.Id }, inquiry);
    }

    [HttpGet, Authorize]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await inquiryService.GetInquiriesAsync(page, pageSize, status, search, from, to);
        return Ok(result);
    }

    [HttpGet("{id:int}"), Authorize]
    public async Task<IActionResult> Get(int id)
    {
        var inquiry = await inquiryService.GetInquiryAsync(id);
        return inquiry is null ? NotFound() : Ok(inquiry);
    }

    [HttpPut("{id:int}/status"), Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateInquiryStatusDto dto)
    {
        var validStatuses = new[] { "New", "InProgress", "Resolved" };
        if (!validStatuses.Contains(dto.Status))
            return BadRequest(new { message = "Invalid status." });

        var inquiry = await inquiryService.UpdateStatusAsync(id, dto.Status);
        return inquiry is null ? NotFound() : Ok(inquiry);
    }

    [HttpGet("stats"), Authorize]
    public async Task<IActionResult> Stats()
    {
        var stats = await inquiryService.GetStatsAsync();
        return Ok(stats);
    }
}
