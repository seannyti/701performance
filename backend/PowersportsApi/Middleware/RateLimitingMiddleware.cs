using System.Collections.Concurrent;
using System.Net;

namespace PowersportsApi.Middleware;

/// <summary>
/// Middleware that implements rate limiting to protect API endpoints from abuse.
/// Uses a sliding window algorithm with configurable limits per endpoint and IP address.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RequestTracker> _requestTrackers = new();
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private DateTime _lastCleanup = DateTime.UtcNow;

    // Rate limit configurations: requests per minute
    private readonly Dictionary<string, int> _endpointLimits = new()
    {
        { "/api/v1/auth/login", 5 },           // 5 login attempts per minute
        { "/api/v1/auth/register", 3 },        // 3 registrations per minute
        { "/api/v1/auth/refresh-token", 10 },  // 10 token refreshes per minute
        { "default", 60 }                      // 60 requests per minute for other endpoints
    };

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Perform periodic cleanup of old entries
        if (DateTime.UtcNow - _lastCleanup > _cleanupInterval)
        {
            CleanupOldEntries();
        }

        var ipAddress = GetClientIpAddress(context);
        var endpoint = context.Request.Path.ToString().ToLowerInvariant();
        var key = $"{ipAddress}:{endpoint}";

        // Get or create tracker for this client+endpoint combination
        var tracker = _requestTrackers.GetOrAdd(key, _ => new RequestTracker());

        // Get rate limit for this endpoint
        var limit = GetRateLimitForEndpoint(endpoint);

        bool rateLimitExceeded = false;
        int retryAfter = 0;

        lock (tracker)
        {
            // Remove requests older than 1 minute
            tracker.RequestTimestamps.RemoveAll(t => DateTime.UtcNow - t > TimeSpan.FromMinutes(1));

            // Check if rate limit exceeded
            if (tracker.RequestTimestamps.Count >= limit)
            {
                rateLimitExceeded = true;
                retryAfter = 60 - (int)(DateTime.UtcNow - tracker.RequestTimestamps.First()).TotalSeconds;
                
                _logger.LogWarning("Rate limit exceeded for IP {IpAddress} on endpoint {Endpoint}. Requests: {Count}/{Limit}",
                    ipAddress, endpoint, tracker.RequestTimestamps.Count, limit);
            }
            else
            {
                // Add current request timestamp
                tracker.RequestTimestamps.Add(DateTime.UtcNow);
            }
        }

        if (rateLimitExceeded)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers["Retry-After"] = retryAfter.ToString();
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = $"Rate limit exceeded. Please try again in {retryAfter} seconds.",
                retryAfter = retryAfter
            });
            
            return;
        }

        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP (when behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private int GetRateLimitForEndpoint(string endpoint)
    {
        // Try exact match first
        if (_endpointLimits.TryGetValue(endpoint, out var limit))
        {
            return limit;
        }

        // Check for partial matches (e.g., all /api/v1/auth/* endpoints)
        foreach (var kvp in _endpointLimits)
        {
            if (kvp.Key != "default" && endpoint.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        // Return default limit
        return _endpointLimits["default"];
    }

    private void CleanupOldEntries()
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-5);
        var keysToRemove = new List<string>();

        foreach (var kvp in _requestTrackers)
        {
            lock (kvp.Value)
            {
                // Remove timestamps older than 5 minutes
                kvp.Value.RequestTimestamps.RemoveAll(t => t < cutoffTime);

                // If no recent requests, mark for removal
                if (kvp.Value.RequestTimestamps.Count == 0)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
        }

        // Remove inactive trackers
        foreach (var key in keysToRemove)
        {
            _requestTrackers.TryRemove(key, out _);
        }

        _lastCleanup = DateTime.UtcNow;
        _logger.LogDebug("Rate limiting cleanup completed. Removed {Count} inactive trackers", keysToRemove.Count);
    }

    private class RequestTracker
    {
        public List<DateTime> RequestTimestamps { get; } = new();
    }
}
