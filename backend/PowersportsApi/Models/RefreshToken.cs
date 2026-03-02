using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models;

public class RefreshToken
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public DateTime ExpiryDate { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Computed property to check if token is still valid
    public bool IsActive => DateTime.UtcNow <= ExpiryDate;
}