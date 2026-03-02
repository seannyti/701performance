namespace PowersportsApi.Middleware;

/// <summary>
/// Middleware that adds security headers to all HTTP responses to protect against
/// common web vulnerabilities including XSS, clickjacking, MIME sniffing, and more.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;
    private readonly bool _isDevelopment;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _isDevelopment = environment.IsDevelopment();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Content Security Policy - Prevents XSS attacks
        var csp = _isDevelopment
            ? "default-src 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:* ws://localhost:*; " +
              "img-src 'self' data: blob: http://localhost:*; " +
              "font-src 'self' data:; " +
              "connect-src 'self' http://localhost:* ws://localhost:*"
            : "default-src 'self'; " +
              "script-src 'self' 'unsafe-inline'; " +
              "style-src 'self' 'unsafe-inline'; " +
              "img-src 'self' data: blob:; " +
              "font-src 'self' data:; " +
              "connect-src 'self'; " +
              "frame-ancestors 'none'; " +
              "base-uri 'self'; " +
              "form-action 'self'";
        
        context.Response.Headers["Content-Security-Policy"] = csp;

        // Prevent clickjacking attacks
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // Prevent MIME type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // Enable XSS protection in browsers
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer policy - control how much referrer information is sent
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions policy - control which browser features can be used
        context.Response.Headers["Permissions-Policy"] = 
            "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), accelerometer=(), gyroscope=()";

        // HSTS - Force HTTPS (only in production)
        if (!_isDevelopment)
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        // Remove server header to avoid exposing server info
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        await _next(context);
    }
}
