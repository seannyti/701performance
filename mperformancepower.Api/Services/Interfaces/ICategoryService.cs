using mperformancepower.Api.DTOs.Category;

namespace mperformancepower.Api.Services.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(bool activeOnly = false);
    Task<CategoryDto?> GetAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<CategoryDto?> ToggleActiveAsync(int id);
    Task<bool> DeleteAsync(int id);
}
