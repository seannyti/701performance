namespace mperformancepower.Api.DTOs.Auth;

public class RegisterResponseDto
{
    /// <summary>True when the user must verify their email before they can log in.</summary>
    public bool RequiresVerification { get; set; }

    /// <summary>Populated only when RequiresVerification is false (immediate auto-login).</summary>
    public AuthResponseDto? Auth { get; set; }
}
