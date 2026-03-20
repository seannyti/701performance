using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using mperformancepower.Api.DTOs.Auth;
using mperformancepower.Api.Services.Interfaces;

namespace mperformancepower.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        if (result is null)
            return Unauthorized(new { message = "Invalid email or password, account is inactive, or email not yet verified." });

        return Ok(result);
    }

    [HttpPost("register"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var (result, error) = await authService.RegisterAsync(dto);
        if (error is not null)
            return Conflict(new { message = error });

        return Ok(result);
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Token is required." });

        var (success, error) = await authService.VerifyEmailAsync(token);
        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Email verified successfully. You can now log in." });
    }
}
