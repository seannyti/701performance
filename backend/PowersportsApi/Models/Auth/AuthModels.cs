using System.ComponentModel.DataAnnotations;

namespace PowersportsApi.Models.Auth;

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number.")]
    public string Password { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    public bool SubscribeNewsletter { get; set; } = false;
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserResponse User { get; set; } = null!;
    public bool RequiresEmailVerification { get; set; } = false;
}

public class VerifyEmailRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class ResendVerificationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }

    public static AuthServiceResult<T> Success(T data)
    {
        return new AuthServiceResult<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static AuthServiceResult<T> Failure(string errorMessage)
    {
        return new AuthServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

public class UserResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
public class ContactRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(200, MinimumLength = 2)]
    public string? Subject { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;
}