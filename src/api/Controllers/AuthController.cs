using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using PerformancePower.Api.Data;
using PerformancePower.Api.DTOs;
using PerformancePower.Api.Helpers;
using PerformancePower.Api.Services;

namespace PerformancePower.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AuthController> _logger;
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwt;

    public AuthController(AuthService auth, IWebHostEnvironment env, ILogger<AuthController> logger, AppDbContext db, JwtHelper jwt)
    {
        _auth = auth;
        _env = env;
        _logger = logger;
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> LoginStaff([FromBody] LoginRequest req)
    {
        var (response, refreshToken, error, mfaRequired, mfaToken, mfaSetupRequired) = await _auth.LoginStaffAsync(req.Email, req.Password);
        if (error != null) return Unauthorized(new { message = error });

        if (mfaRequired)
            return Ok(new { mfaRequired = true, mfaToken, mfaSetupRequired });

        SetRefreshTokenCookie(refreshToken!);
        return Ok(response);
    }

    // ── MFA Endpoints ─────────────────────────────────────────────────────────

    /// <summary>Generate TOTP secret + QR URI for authenticated user. Does not enable MFA yet.</summary>
    [Authorize]
    [HttpGet("mfa/setup")]
    public async Task<IActionResult> MfaSetup()
    {
        var userId = User.GetUserId();
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        var secretBytes = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretBytes);

        var siteName = (await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "site_name"))?.Value
            ?? (await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "seo_title"))?.Value
            ?? "PerformancePower";

        var issuer = Uri.EscapeDataString(siteName);
        var account = Uri.EscapeDataString(user.Email);
        var otpUri = $"otpauth://totp/{issuer}:{account}?secret={base32Secret}&issuer={issuer}";

        user.TotpSecret = base32Secret;
        await _db.SaveChangesAsync();

        return Ok(new { otpUri, secret = base32Secret });
    }

    /// <summary>Verify a TOTP code and enable MFA for the authenticated user.</summary>
    [Authorize]
    [HttpPost("mfa/enable")]
    public async Task<IActionResult> MfaEnable([FromBody] MfaCodeRequest req)
    {
        var userId = User.GetUserId();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();
        if (string.IsNullOrEmpty(user.TotpSecret))
            return BadRequest(new { message = "Call /api/auth/mfa/setup first." });

        var totp = new Totp(Base32Encoding.ToBytes(user.TotpSecret));
        if (!totp.VerifyTotp(req.Code, out _, VerificationWindow.RfcSpecifiedNetworkDelay))
            return BadRequest(new { message = "Invalid verification code." });

        user.MfaEnabled = true;
        await _db.SaveChangesAsync();
        return Ok(new { message = "MFA enabled successfully." });
    }

    /// <summary>Disable MFA for the authenticated user (requires current TOTP code).</summary>
    [Authorize]
    [HttpPost("mfa/disable")]
    public async Task<IActionResult> MfaDisable([FromBody] MfaCodeRequest req)
    {
        var userId = User.GetUserId();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();
        if (!user.MfaEnabled || string.IsNullOrEmpty(user.TotpSecret))
            return BadRequest(new { message = "MFA is not enabled." });

        var totp = new Totp(Base32Encoding.ToBytes(user.TotpSecret));
        if (!totp.VerifyTotp(req.Code, out _, VerificationWindow.RfcSpecifiedNetworkDelay))
            return BadRequest(new { message = "Invalid verification code." });

        user.MfaEnabled = false;
        user.TotpSecret = null;
        await _db.SaveChangesAsync();
        return Ok(new { message = "MFA disabled." });
    }

    /// <summary>Complete login: verify TOTP code using the mfa-pending token issued during /login.</summary>
    [HttpPost("mfa/verify-login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> MfaVerifyLogin([FromBody] MfaVerifyLoginRequest req)
    {
        var userId = _jwt.ValidateMfaToken(req.MfaToken);
        if (userId == null) return Unauthorized(new { message = "Invalid or expired MFA session." });

        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || !user.MfaEnabled || string.IsNullOrEmpty(user.TotpSecret))
            return Unauthorized(new { message = "MFA not configured for this account." });

        var totp = new Totp(Base32Encoding.ToBytes(user.TotpSecret));
        if (!totp.VerifyTotp(req.Code, out _, VerificationWindow.RfcSpecifiedNetworkDelay))
            return Unauthorized(new { message = "Invalid verification code." });

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshToken = await _auth.CreateRefreshTokenForUserAsync(user.Id);
        SetRefreshTokenCookie(refreshToken);

        return Ok(new AuthResponse(accessToken, user.Role.Name, user.Id, user.FirstName, user.LastName, user.Email));
    }

    /// <summary>Returns MFA status for the current user.</summary>
    [Authorize]
    [HttpGet("mfa/status")]
    public async Task<IActionResult> MfaStatus()
    {
        var userId = User.GetUserId();
        var user = await _db.Users.Select(u => new { u.Id, u.MfaEnabled }).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();
        return Ok(new { mfaEnabled = user.MfaEnabled });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var token = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        var (accessToken, newRefreshToken) = await _auth.RefreshTokenAsync(token);
        if (accessToken == null) return Unauthorized();

        SetRefreshTokenCookie(newRefreshToken!);
        return Ok(new RefreshResponse(accessToken));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(token))
            await _auth.RevokeRefreshTokenAsync(token);

        Response.Cookies.Delete("refreshToken");
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.GetUserId();
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        var firstName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "";
        var lastName = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

        return Ok(new UserProfileResponse(userId, firstName, lastName, email, null, role));
    }

    private void SetRefreshTokenCookie(string token)
    {
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7)
        });
    }
}
