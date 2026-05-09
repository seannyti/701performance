namespace PerformancePower.Api.DTOs;

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string AccessToken,
    string Role,
    int UserId,
    string FirstName,
    string LastName,
    string Email
);

public record RefreshResponse(string AccessToken);

public record MfaCodeRequest(string Code);

public record MfaVerifyLoginRequest(string MfaToken, string Code);

public record MfaEnableRequiredRequest(string MfaToken, string Code);

public record UserProfileResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Role
);
