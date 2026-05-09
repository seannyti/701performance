using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PerformancePower.Api.Middleware;

/// <summary>
/// Catches any unhandled exception and returns a RFC 7807 ProblemDetails JSON response
/// instead of the default ASP.NET HTML error page. Includes a traceId for correlation.
/// </summary>
public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger,
    IWebHostEnvironment env)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        // Don't overwrite a response that has already started streaming
        if (context.Response.HasStarted) return;

        var (status, title) = ex switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            KeyNotFoundException        => (StatusCodes.Status404NotFound,    "Resource not found"),
            ArgumentException           => (StatusCodes.Status400BadRequest,  "Bad request"),
            InvalidOperationException   => (StatusCodes.Status400BadRequest,  "Invalid operation"),
            _                           => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode  = status;
        context.Response.ContentType = "application/problem+json";

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var problem = new ProblemDetails
        {
            Type     = $"https://httpstatuses.io/{status}",
            Title    = title,
            Status   = status,
            Instance = context.Request.Path,
            // Only expose full exception detail in development
            Detail   = env.IsDevelopment() ? ex.ToString() : ex.Message
        };

        problem.Extensions["traceId"] = traceId;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, _jsonOptions));
    }
}
