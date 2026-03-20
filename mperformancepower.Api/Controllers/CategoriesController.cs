using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mperformancepower.Api.DTOs.Category;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    // Public: returns only active categories (for filters, dropdowns)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
    {
        var categories = await categoryService.GetAllAsync(activeOnly);
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var category = await categoryService.GetAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var category = await categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await categoryService.UpdateAsync(id, dto);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPatch("{id:int}/toggle"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Toggle(int id)
    {
        var category = await categoryService.ToggleActiveAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await categoryService.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
