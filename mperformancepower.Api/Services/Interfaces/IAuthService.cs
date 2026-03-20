using mperformancepower.Api.DTOs.Auth;

namespace mperformancepower.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<(RegisterResponseDto? response, string? error)> RegisterAsync(RegisterDto dto);
    Task<(bool success, string? error)> VerifyEmailAsync(string token);
}
