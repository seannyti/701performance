using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using PowersportsApi.Data;
using PowersportsApi.Models;
using PowersportsApi.Models.Auth;

namespace PowersportsApi.Services;

/// <summary>
/// Service for handling user authentication, registration, and token management.
/// </summary>
public class AuthService
{
    private readonly PowersportsDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly EmailService _emailService;

    public AuthService(PowersportsDbContext context, IConfiguration configuration, ILogger<AuthService> logger, EmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _emailService = emailService;
    }

    /// <summary>
    /// Registers a new user account with the provided details.
    /// </summary>
    /// <param name="request">The registration request containing user details.</param>
    /// <returns>An authentication response with tokens if successful, or an error message.</returns>
    public async Task<AuthServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return AuthServiceResult<AuthResponse>.Failure("A user with this email address already exists.");
            }

            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email.ToLowerInvariant(),
                PasswordHash = passwordHash,
                Phone = request.Phone,
                SubscribeNewsletter = request.SubscribeNewsletter,
                Role = UserRole.User, // Default role for new registrations
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Check if email verification is required
            var requireVerificationSetting = await _context.SiteSettings
                .Where(s => s.Key == "require_email_verification")
                .Select(s => s.Value)
                .FirstOrDefaultAsync();
            bool requireVerification = requireVerificationSetting?.ToLowerInvariant() == "true";

            if (requireVerification)
            {
                // Generate verification token
                var verificationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
                user.IsEmailVerified = false;
                user.EmailVerificationToken = verificationToken;
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            }
            else
            {
                user.IsEmailVerified = true;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Email}", request.Email);

            if (requireVerification)
            {
                // Send verification email (best-effort — don't fail registration if email fails)
                try
                {
                    var siteUrl = await _context.SiteSettings
                        .Where(s => s.Key == "site_url")
                        .Select(s => s.Value)
                        .FirstOrDefaultAsync() ?? "http://localhost:3000";
                    await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, user.EmailVerificationToken!, siteUrl);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send verification email to {Email}", user.Email);
                }

                return AuthServiceResult<AuthResponse>.Success(new AuthResponse
                {
                    RequiresEmailVerification = true,
                    Token = string.Empty,
                    RefreshToken = string.Empty,
                    ExpiresAt = DateTime.UtcNow,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Phone = user.Phone,
                        FullName = user.FullName,
                        Role = user.Role,
                        RoleName = user.Role.GetRoleName(),
                        CreatedAt = user.CreatedAt
                    }
                });
            }

            // Generate tokens
            var accessExpiryMinutesReg = GetAccessTokenExpiryMinutes();
            var accessToken = GenerateAccessToken(user, accessExpiryMinutesReg);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

            var response = new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(accessExpiryMinutesReg),
                User = new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    FullName = user.FullName,
                    Role = user.Role,
                    RoleName = user.Role.GetRoleName(),
                    CreatedAt = user.CreatedAt
                }
            };

            return AuthServiceResult<AuthResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration: {Email}", request.Email);
            return AuthServiceResult<AuthResponse>.Failure("An error occurred during registration. Please try again.");
        }
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <returns>An authentication response with tokens if successful, or an error message.</returns>
    public async Task<AuthServiceResult<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress = null)
    {
        try
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());
            if (user == null || !user.IsActive)
            {
                return AuthServiceResult<AuthResponse>.Failure("Invalid email or password.");
            }

            // Check if account is currently locked out
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remaining = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                return AuthServiceResult<AuthResponse>.Failure($"Account is locked due to too many failed login attempts. Please try again in {remaining} minute(s).");
            }

            // Check email verification
            var requireVerifySetting = await _context.SiteSettings
                .Where(s => s.Key == "require_email_verification")
                .Select(s => s.Value)
                .FirstOrDefaultAsync();
            if (requireVerifySetting?.ToLowerInvariant() == "true" && !user.IsEmailVerified)
            {
                return AuthServiceResult<AuthResponse>.Failure("EMAIL_NOT_VERIFIED");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // Increment failed attempts and check against max_login_attempts setting
                user.FailedLoginAttempts++;
                var maxAttemptsSetting = await _context.SiteSettings
                    .Where(s => s.Key == "max_login_attempts")
                    .Select(s => s.Value)
                    .FirstOrDefaultAsync();
                int maxAttempts = int.TryParse(maxAttemptsSetting, out int ma) && ma > 0 ? ma : 5;

                if (user.FailedLoginAttempts >= maxAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15); // 15-minute lockout
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogWarning("Account locked after {Attempts} failed attempts: {Email}", user.FailedLoginAttempts, request.Email);
                    return AuthServiceResult<AuthResponse>.Failure($"Account locked after {maxAttempts} failed attempts. Please try again in 15 minutes.");
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return AuthServiceResult<AuthResponse>.Failure("Invalid email or password.");
            }

            // Successful login — reset lockout state
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            // Update last login time and IP
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(ipAddress))
                user.LastLoginIp = ipAddress;

            // Remove old refresh tokens for this user
            var oldRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id)
                .ToListAsync();
            _context.RefreshTokens.RemoveRange(oldRefreshTokens);

            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged in: {Email}", request.Email);

            // Generate tokens
            var accessExpiryMinutes = GetAccessTokenExpiryMinutes();
            var accessToken = GenerateAccessToken(user, accessExpiryMinutes);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

            var response = new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(accessExpiryMinutes),
                User = new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    FullName = user.FullName,
                    Role = user.Role,
                    RoleName = user.Role.GetRoleName(),
                    CreatedAt = user.CreatedAt
                }
            };

            return AuthServiceResult<AuthResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login: {Email}", request.Email);
            return AuthServiceResult<AuthResponse>.Failure("An error occurred during login. Please try again.");
        }
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh token request containing the refresh token.</param>
    /// <returns>A new authentication response with fresh tokens if successful, or an error message.</returns>
    public async Task<AuthServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            var refreshTokenEntity = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshTokenEntity == null || !refreshTokenEntity.IsActive)
            {
                return AuthServiceResult<AuthResponse>.Failure("Invalid refresh token.");
            }

            var user = refreshTokenEntity.User;
            if (!user.IsActive)
            {
                return AuthServiceResult<AuthResponse>.Failure("User account is not active.");
            }

            // Remove old refresh token
            _context.RefreshTokens.Remove(refreshTokenEntity);

            // Generate new tokens
            var accessExpiryMinutesRefresh = GetAccessTokenExpiryMinutes();
            var newAccessToken = GenerateAccessToken(user, accessExpiryMinutesRefresh);
            var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tokens refreshed for user: {UserId}", user.Id);

            var response = new AuthResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(accessExpiryMinutesRefresh),
                User = new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    FullName = user.FullName,
                    Role = user.Role,
                    RoleName = user.Role.GetRoleName(),
                    CreatedAt = user.CreatedAt
                }
            };

            return AuthServiceResult<AuthResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return AuthServiceResult<AuthResponse>.Failure("An error occurred during token refresh. Please login again.");
        }
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token.
    /// </summary>
    /// <param name="request">The logout request containing the refresh token to invalidate.</param>
    /// <returns>A result indicating successful logout or an error message.</returns>
    public async Task<AuthServiceResult<object>> LogoutAsync(LogoutRequest request)
    {
        try
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("User logged out: {UserId}", refreshToken.UserId);
            }

            return AuthServiceResult<object>.Success(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return AuthServiceResult<object>.Failure("An error occurred during logout.");
        }
    }

    /// <summary>
    /// Retrieves a user's information by their ID.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve.</param>
    /// <returns>The user's information, or null if not found or inactive.</returns>
    public async Task<UserResponse?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        if (user == null)
        {
            return null;
        }

        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            FullName = user.FullName,
            Role = user.Role,
            RoleName = user.Role.GetRoleName(),
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>
    /// Removes expired refresh tokens from the database to maintain database hygiene.
    /// </summary>
    /// <returns>A result containing the number of removed tokens, or an error message.</returns>
    public async Task<AuthServiceResult<object>> CleanupExpiredTokensAsync()
    {
        try
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiryDate < DateTime.UtcNow)
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired refresh tokens", expiredTokens.Count);
            
            return AuthServiceResult<object>.Success(new { removedCount = expiredTokens.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token cleanup");
            return AuthServiceResult<object>.Failure("An error occurred during token cleanup.");
        }
    }

    private string GenerateAccessToken(User user, int expiryMinutes)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? jwtSettings["SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("JWT secret key is not configured. Set the JWT_SECRET environment variable.");
        var issuer = jwtSettings["Issuer"] ?? "PowersportsApi";
        var audience = jwtSettings["Audience"] ?? "PowersportsApp";

        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Role, user.Role.GetRoleName())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>Issues a fresh refresh token without removing any existing ones (for multi-client support).</summary>
    public Task<string> IssueRefreshTokenAsync(int userId) => GenerateAndSaveRefreshTokenAsync(userId);

    private async Task<string> GenerateAndSaveRefreshTokenAsync(int userId)
    {
        var refreshToken = GenerateSecureRefreshToken();
        // Refresh token lifetime is controlled by session_timeout — when it expires
        // the user must re-authenticate, enforcing the admin-configured session boundary.
        var sessionTimeoutMinutes = await GetSessionTimeoutMinutesAsync();

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddMinutes(sessionTimeoutMinutes),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    private static string GenerateSecureRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    // Short-lived JWT duration — read from config, not the DB session_timeout.
    private int GetAccessTokenExpiryMinutes() =>
        int.Parse(_configuration["JwtSettings:AccessTokenExpiryMinutes"] ?? "60");

    // Maximum session length — controls how long the refresh token lives.
    // When the refresh token expires the user must re-authenticate, enforcing
    // the admin-configured session boundary.
    private async Task<int> GetSessionTimeoutMinutesAsync()
    {
        var setting = await _context.SiteSettings
            .Where(s => s.Key == "session_timeout")
            .Select(s => s.Value)
            .FirstOrDefaultAsync();
        if (int.TryParse(setting, out int timeout) && timeout > 0)
            return timeout;
        return 480; // default 8-hour session if setting is missing
    }
}