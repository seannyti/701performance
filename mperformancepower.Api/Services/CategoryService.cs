using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Data;
using mperformancepower.Api.DTOs.Category;
using mperformancepower.Api.Models;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Services;

public class CategoryService(AppDbContext db) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync(bool activeOnly = false)
    {
        var query = db.Categories.Include(c => c.Vehicles).AsQueryable();
        if (activeOnly)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive,
                VehicleCount = c.Vehicles.Count
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetAsync(int id)
    {
        var c = await db.Categories.Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Id == id);
        return c is null ? null : MapToDto(c);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category { Name = dto.Name };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await db.Categories.Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return null;

        category.Name = dto.Name;
        await db.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<CategoryDto?> ToggleActiveAsync(int id)
    {
        var category = await db.Categories.Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return null;

        category.IsActive = !category.IsActive;
        await db.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await db.Categories.Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return false;
        if (category.Vehicles.Count > 0) throw new InvalidOperationException("Cannot delete a category that has vehicles.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return true;
    }

    private static CategoryDto MapToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        IsActive = c.IsActive,
        VehicleCount = c.Vehicles.Count
    };
}
