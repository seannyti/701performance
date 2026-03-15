using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models;

/// <summary>
/// Junction table linking Categories to MediaFiles (typically one image per category)
/// </summary>
public class CategoryImage
{
    public int Id { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    [Required]
    public int MediaFileId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Category Category { get; set; } = null!;
    public MediaFile MediaFile { get; set; } = null!;
}