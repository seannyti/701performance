using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models;

/// <summary>
/// Represents an uploaded media file (image, video, document, etc.)
/// </summary>
public class MediaFile
{
    public int Id { get; set; }
    
    /// <summary>
    /// Original filename when uploaded
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Stored filename (may be sanitized/hashed)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string StoredFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// File path relative to wwwroot/uploads
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Thumbnail path (for images)
    /// </summary>
    [MaxLength(1000)]
    public string? ThumbnailPath { get; set; }
    
    /// <summary>
    /// MIME type (e.g., image/jpeg, image/png)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string MimeType { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Media type category (Image, Video, Document, etc.)
    /// </summary>
    public MediaType MediaType { get; set; }
    
    /// <summary>
    /// Image width in pixels (for images)
    /// </summary>
    public int? Width { get; set; }
    
    /// <summary>
    /// Image height in pixels (for images)
    /// </summary>
    public int? Height { get; set; }
    
    /// <summary>
    /// Alt text for accessibility
    /// </summary>
    [MaxLength(500)]
    public string? AltText { get; set; }
    
    /// <summary>
    /// Optional caption or description
    /// </summary>
    [MaxLength(1000)]
    public string? Caption { get; set; }
    
    /// <summary>
    /// Tags for searching/filtering (comma-separated)
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    /// <summary>
    /// Optional section/folder for organization
    /// </summary>
    public int? SectionId { get; set; }
    
    /// <summary>
    /// User who uploaded the file
    /// </summary>
    public int? UploadedByUserId { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public User? UploadedByUser { get; set; }
    public MediaSection? Section { get; set; }
}

/// <summary>
/// Types of media files
/// </summary>
public enum MediaType
{
    Image = 0,
    Video = 1,
    Document = 2,
    Other = 3
}
