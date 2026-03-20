using System.ComponentModel.DataAnnotations;

namespace mperformancepower.Api.DTOs.Category;

public class UpdateCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
