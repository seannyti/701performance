using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models;

/// <summary>
/// Dynamic site settings and content management
/// Allows admins to edit site content without code changes
/// </summary>
public class SiteSetting
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique key identifier for the setting (e.g., "footer.company_name", "homepage.hero_title")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for the admin interface
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Current value of the setting
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// Description explaining what this setting controls
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Type of setting for appropriate UI rendering
    /// </summary>
    public SettingType Type { get; set; } = SettingType.Text;
    
    /// <summary>
    /// Category for organizing settings in admin interface
    /// </summary>
    [MaxLength(50)]
    public string Category { get; set; } = "General";
    
    /// <summary>
    /// Display order within category
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this setting is required (cannot be empty)
    /// </summary>
    public bool IsRequired { get; set; } = false;
    
    /// <summary>
    /// Whether this setting is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// When this setting was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this setting was last modified
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User ID who last modified this setting
    /// </summary>
    public int? LastModifiedBy { get; set; }
    
    /// <summary>
    /// Navigation property to user who last modified
    /// </summary>
    public User? LastModifiedByUser { get; set; }
}

/// <summary>
/// Types of settings for appropriate UI rendering
/// </summary>
public enum SettingType
{
    /// <summary>
    /// Single line text input
    /// </summary>
    Text = 0,
    
    /// <summary>
    /// Multi-line text area
    /// </summary>
    TextArea = 1,
    
    /// <summary>
    /// Rich HTML content
    /// </summary>
    Html = 2,
    
    /// <summary>
    /// Email address
    /// </summary>
    Email = 3,
    
    /// <summary>
    /// Phone number
    /// </summary>
    Phone = 4,
    
    /// <summary>
    /// URL/Website link
    /// </summary>
    Url = 5,
    
    /// <summary>
    /// Image file path or URL
    /// </summary>
    Image = 6,
    
    /// <summary>
    /// True/false toggle
    /// </summary>
    Boolean = 7,
    
    /// <summary>
    /// Numeric value
    /// </summary>
    Number = 8,
    
    /// <summary>
    /// Color picker value
    /// </summary>
    Color = 9
}