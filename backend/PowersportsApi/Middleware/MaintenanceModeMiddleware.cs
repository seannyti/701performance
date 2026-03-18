using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PowersportsApi.Data;

namespace PowersportsApi.Middleware;

/// <summary>
/// Returns 503 Service Unavailable for all non-whitelisted API routes when
/// maintenance mode is enabled. The setting is cached for 30 seconds to avoid
/// hitting the database on every request.
/// </summary>
public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "maintenance_mode_active";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    // Paths that are always reachable, even during maintenance.
    // /api/v1/settings   — frontend needs this to know maintenance is active
    // /api/v1/auth/      — admins must be able to log in
    // /api/v1/admin/     — admins must be able to manage the site
    // /hubs/             — SignalR (admin live chat)
    // /health            — uptime monitoring
    // /swagger           — API documentation
    private static readonly string[] AllowedPrefixes =
    [
        "/api/v1/settings",
        "/api/v1/auth/",
        "/api/v1/admin/",
        "/hubs/",
        "/health",
        "/swagger",
    ];

    public MaintenanceModeMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context, PowersportsDbContext db)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (IsAllowed(path))
        {
            await _next(context);
            return;
        }

        var isMaintenanceMode = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var value = await db.SiteSettings
                .Where(s => s.Key == "enable_maintenance_mode")
                .Select(s => s.Value)
                .FirstOrDefaultAsync();
            return value?.ToLowerInvariant() == "true";
        });

        if (isMaintenanceMode)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers.RetryAfter = "3600";
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"message\":\"The site is temporarily under maintenance. Please try again later.\",\"status\":503}");
            return;
        }

        await _next(context);
    }

    private static bool IsAllowed(string path) =>
        AllowedPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
