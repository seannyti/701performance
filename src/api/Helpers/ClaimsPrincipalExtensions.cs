using System.Security.Claims;

namespace PerformancePower.Api.Helpers;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the authenticated staff user's integer ID from the "userId" JWT claim.
    /// Throws UnauthorizedAccessException (→ 401) instead of crashing with NullReferenceException.
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("userId")?.Value;
        if (value == null || !int.TryParse(value, out var id))
            throw new UnauthorizedAccessException("Missing or invalid userId claim.");
        return id;
    }

}
