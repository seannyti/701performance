using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    public bool SubscribeNewsletter { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }

    [MaxLength(45)]
    public string? LastLoginIp { get; set; }
    
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the user has verified their email address
    /// </summary>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>
    /// Token sent to the user to verify their email address
    /// </summary>
    [MaxLength(200)]
    public string? EmailVerificationToken { get; set; }

    /// <summary>
    /// When the email verification token expires
    /// </summary>
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    
    /// <summary>
    /// Number of consecutive failed login attempts since last successful login
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;
    
    /// <summary>
    /// When set, the account is locked until this UTC time due to too many failed attempts
    /// </summary>
    public DateTime? LockoutEnd { get; set; }
    
    /// <summary>
    /// User role for authorization and permission management
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;
    
    // Navigation property for refresh tokens
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    // Computed property for display
    public string FullName => $"{FirstName} {LastName}";
}