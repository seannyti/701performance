using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using mperformancepower.Api.Data;
using mperformancepower.Api.DTOs.Auth;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Services;

public class AuthService(AppDbContext db, IConfiguration config, IMailService mailService) : IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        if (!user.IsActive)
            return null;

        var authSettings = await GetAuthSettingsAsync();
        if (authSettings.RequireEmailVerification && !user.EmailVerified)
            return null;

        return BuildJwt(user);
    }

    public async Task<(RegisterResponseDto? response, string? error)> RegisterAsync(RegisterDto dto)
    {
        var authSettings = await GetAuthSettingsAsync();

        if (!authSettings.EnableRegistration)
            return (null, "Registration is currently disabled.");

        var exists = await db.AppUsers.AnyAsync(u => u.Email == dto.Email);
        if (exists)
            return (null, "An account with this email already exists.");

        var requireVerification = authSettings.RequireEmailVerification;
        var verificationToken = requireVerification ? Guid.NewGuid().ToString("N") : null;

        var user = new Models.AppUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Customer",
            EmailVerified = !requireVerification,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = requireVerification ? DateTime.UtcNow.AddHours(24) : null,
            IsActive = true,
        };

        db.AppUsers.Add(user);
        await db.SaveChangesAsync();

        if (requireVerification && verificationToken is not null)
        {
            var publicUrl = config["App:PublicUrl"] ?? "https://mperformancepower.com";
            await mailService.SendEmailVerificationAsync(user.Email, user.FirstName ?? user.Email, verificationToken, publicUrl);
            return (new RegisterResponseDto { RequiresVerification = true, Auth = null }, null);
        }

        var auth = BuildJwt(user);
        return (new RegisterResponseDto { RequiresVerification = false, Auth = auth }, null);
    }

    public async Task<(bool success, string? error)> VerifyEmailAsync(string token)
    {
        var user = await db.AppUsers.FirstOrDefaultAsync(u =>
            u.EmailVerificationToken == token &&
            u.EmailVerificationTokenExpiry > DateTime.UtcNow);

        if (user is null)
            return (false, "Verification link is invalid or has expired.");

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await db.SaveChangesAsync();

        return (true, null);
    }

    private AuthResponseDto BuildJwt(Models.AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(24);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var jwtToken = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            ExpiresAt = expires,
            Email = user.Email,
            Role = user.Role,
            FirstName = user.FirstName,
            LastName = user.LastName,
        };
    }

    private async Task<AuthSettings> GetAuthSettingsAsync()
    {
        var row = await db.SiteSettings.FindAsync("auth");
        if (row is null) return new AuthSettings();
        try
        {
            return JsonSerializer.Deserialize<AuthSettings>(row.DataJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new AuthSettings();
        }
        catch { return new AuthSettings(); }
    }

    private sealed record AuthSettings(
        bool EnableRegistration = true,
        bool RequireEmailVerification = false);
}
