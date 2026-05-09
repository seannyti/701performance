using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PerformancePower.Api.Data;
using PerformancePower.Api.DTOs;
using PerformancePower.Api.Helpers;
using PerformancePower.Api.Models;
using Serilog;

namespace PerformancePower.Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwt;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, JwtHelper jwt, IConfiguration config)
    {
        _db = db;
        _jwt = jwt;
        _config = config;
    }

    // Staff portal login
    public async Task<(AuthResponse? response, string? refreshToken, string? error, bool mfaRequired, string? mfaToken, bool mfaSetupRequired)> LoginStaffAsync(string email, string password)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (null, null, "Invalid email or password", false, null, false);

        // Only require MFA code if the user has explicitly enabled it
        if (user.MfaEnabled)
        {
            var mfaToken = _jwt.GenerateMfaToken(user.Id);
            return (null, null, null, true, mfaToken, false);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return (new AuthResponse(accessToken, user.Role.Name, user.Id, user.FirstName, user.LastName, user.Email), refreshToken.Token, null, false, null, false);
    }

    public async Task<(string? accessToken, string? newRefreshToken)> RefreshTokenAsync(string token)
    {
        var refreshToken = await _db.RefreshTokens
            .Include(r => r.User).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(r => r.Token == token);

        if (refreshToken == null || !refreshToken.IsActive)
            return (null, null);

        // Rotate — revoke old, issue new
        refreshToken.RevokedAt = DateTime.UtcNow;
        var newToken = await CreateRefreshTokenAsync(refreshToken.UserId);

        return (_jwt.GenerateAccessToken(refreshToken.User), newToken.Token);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
        if (refreshToken == null || !refreshToken.IsActive) return false;

        refreshToken.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<string> CreateRefreshTokenForUserAsync(int userId)
    {
        var token = await CreateRefreshTokenAsync(userId);
        return token.Token;
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(int userId)
    {
        var days = int.Parse(_config["Jwt:RefreshDays"] ?? "7");
        var token = new RefreshToken
        {
            UserId = userId,
            Token = _jwt.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(days)
        };
        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();
        return token;
    }

    public async Task SeedAdminAsync()
    {
        if (await _db.Users.AnyAsync()) return;

        var adminRole = await _db.Roles.FirstAsync(r => r.Name == "admin");

        // In testing, use a fixed seed password from config so tests can authenticate predictably.
        // In production, generate a cryptographically random password printed once to the startup log.
        var tempPassword = _config["Admin:SeedPassword"];
        if (string.IsNullOrEmpty(tempPassword))
        {
            var rawBytes = RandomNumberGenerator.GetBytes(12);
            tempPassword = Convert.ToBase64String(rawBytes).Replace('+', '-').Replace('/', '_') + "!";
        }

        var admin = new User
        {
            Email = "admin@performancepower.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            FirstName = "Admin",
            LastName = "User",
            RoleId = adminRole.Id,
            IsActive = true
        };
        _db.Users.Add(admin);
        await _db.SaveChangesAsync();

        Log.Warning("=== ADMIN SEEDED === Email: admin@performancepower.com | Temp password: {Password} | Change this immediately.", tempPassword);
    }
}
