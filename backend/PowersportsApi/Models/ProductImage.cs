using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models;

/// <summary>
/// Junction table linking Products to MediaFiles for image galleries
/// </summary>
public class ProductImage
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public int MediaFileId { get; set; }
    
    public bool IsMain { get; set; } = false;
    
    public int SortOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Product Product { get; set; } = null!;
    public MediaFile MediaFile { get; set; } = null!;
}