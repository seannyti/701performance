using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models;

/// <summary>
/// Represents a section/folder for organizing media files
/// </summary>
public class MediaSection
{
    public int Id { get; set; }
    
    /// <summary>
    /// Section name (e.g., "ATVs", "Team Members", "Promotions")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description of the section
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Display order for sorting sections
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this is a system-created section (cannot be deleted)
    /// </summary>
    public bool IsSystem { get; set; } = false;
    
    /// <summary>
    /// Whether this section is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Optional category ID if this section is auto-created from a category
    /// </summary>
    public int? CategoryId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
}
