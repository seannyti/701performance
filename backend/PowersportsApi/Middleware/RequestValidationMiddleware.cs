using System.Text.RegularExpressions;

namespace PowersportsApi.Middleware;

/// <summary>
/// Middleware that validates and sanitizes incoming requests to protect against
/// injection attacks, malformed data, and other security threats.
/// </summary>
public class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;

    // Dangerous patterns that indicate potential attacks.
    // SQL keyword detection intentionally omitted — EF Core uses parameterized queries so SQL injection
    // via input is not possible, and keyword matching causes false positives on legitimate product names
    // (e.g. "Select Accessories", "Drop Handlebar Kit").
    private static readonly Regex[] DangerousPatterns = new[]
    {
        new Regex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"javascript:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled), // Event handlers
        new Regex(@"<iframe", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"\.\.[\\/]", RegexOptions.Compiled), // Path traversal
    };

    public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Validate URL path
        if (ContainsDangerousContent(context.Request.Path.Value))
        {
            _logger.LogWarning("Potentially malicious request path detected: {Path} from IP: {IpAddress}",
                context.Request.Path, GetClientIpAddress(context));
            
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid request" });
            return;
        }

        // Validate query string parameters
        foreach (var queryParam in context.Request.Query)
        {
            if (ContainsDangerousContent(queryParam.Value.ToString()))
            {
                _logger.LogWarning("Potentially malicious query parameter detected: {ParamName} from IP: {IpAddress}",
                    queryParam.Key, GetClientIpAddress(context));
                
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid request parameters" });
                return;
            }
        }

        // Validate request headers for common injection attempts
        var suspiciousHeaders = new[] { "User-Agent", "Referer", "X-Forwarded-For" };
        foreach (var headerName in suspiciousHeaders)
        {
            if (context.Request.Headers.TryGetValue(headerName, out var headerValue))
            {
                foreach (var value in headerValue)
                {
                    if (ContainsDangerousContent(value))
                    {
                        _logger.LogWarning("Potentially malicious header detected: {HeaderName} from IP: {IpAddress}",
                            headerName, GetClientIpAddress(context));
                        
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new { error = "Invalid request headers" });
                        return;
                    }
                }
            }
        }

        // Check request size limits
        if (context.Request.ContentLength > 50 * 1024 * 1024) // 50MB limit
        {
            _logger.LogWarning("Request exceeds size limit: {Size} bytes from IP: {IpAddress}",
                context.Request.ContentLength, GetClientIpAddress(context));
            
            context.Response.StatusCode = 413;
            await context.Response.WriteAsJsonAsync(new { error = "Request too large" });
            return;
        }

        await _next(context);
    }

    private static bool ContainsDangerousContent(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        foreach (var pattern in DangerousPatterns)
        {
            if (pattern.IsMatch(input))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Use the TCP-level remote address — not client-supplied headers, which are trivially spoofed.
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
