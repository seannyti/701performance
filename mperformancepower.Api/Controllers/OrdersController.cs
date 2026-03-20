using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mperformancepower.Api.DTOs.Order;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var result = await orderService.GetOrdersAsync(page, pageSize, status, search);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var order = await orderService.GetOrderAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var order = await orderService.CreateOrderAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto dto)
    {
        var order = await orderService.UpdateOrderAsync(id, dto);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await orderService.DeleteOrderAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("finance/stats")]
    public async Task<IActionResult> FinanceStats()
    {
        var stats = await orderService.GetFinanceStatsAsync();
        return Ok(stats);
    }
}
