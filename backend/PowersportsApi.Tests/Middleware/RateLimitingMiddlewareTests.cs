using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PowersportsApi.Middleware;
using System.Text.Json;

namespace PowersportsApi.Tests.Middleware;

/// <summary>
/// Unit tests for RateLimitingMiddleware verifying per-endpoint throttling,
/// IP isolation, and correct HTTP 429 response headers.
/// </summary>
public class RateLimitingMiddlewareTests
{
    private readonly Mock<ILogger<RateLimitingMiddleware>> _loggerMock = new();

    // ─── Happy Path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task NormalRequest_BelowLimit_PassesThrough()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateHttpContext("/api/v1/products", "192.168.1.1");
        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    // ─── Login Rate Limiting ──────────────────────────────────────────────────

    [Fact]
    public async Task LoginEndpoint_BlocksAfterFiveRequests()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // First 5 should pass
        for (var i = 0; i < 5; i++)
        {
            var ctx = CreateHttpContext("/api/v1/auth/login", "10.0.0.1");
            await middleware.InvokeAsync(ctx);
            ctx.Response.StatusCode.Should().Be(200, $"request #{i + 1} should pass");
        }

        // 6th should be blocked
        var blockedCtx = CreateHttpContext("/api/v1/auth/login", "10.0.0.1");
        await middleware.InvokeAsync(blockedCtx);

        blockedCtx.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task LoginEndpoint_DifferentIPsTrackedSeparately()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Exhaust the limit for IP A
        for (var i = 0; i < 5; i++)
            await middleware.InvokeAsync(CreateHttpContext("/api/v1/auth/login", "1.1.1.1"));

        // IP A is blocked
        var blockedCtx = CreateHttpContext("/api/v1/auth/login", "1.1.1.1");
        await middleware.InvokeAsync(blockedCtx);
        blockedCtx.Response.StatusCode.Should().Be(429);

        // IP B should still be allowed
        var allowedCtx = CreateHttpContext("/api/v1/auth/login", "2.2.2.2");
        await middleware.InvokeAsync(allowedCtx);
        allowedCtx.Response.StatusCode.Should().Be(200);
    }

    // ─── Register Rate Limiting ───────────────────────────────────────────────

    [Fact]
    public async Task RegisterEndpoint_BlocksAfterThreeRequests()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // First 3 should pass
        for (var i = 0; i < 3; i++)
        {
            var ctx = CreateHttpContext("/api/v1/auth/register", "10.0.0.2");
            await middleware.InvokeAsync(ctx);
            ctx.Response.StatusCode.Should().Be(200, $"request #{i + 1} should pass");
        }

        // 4th should be blocked (register has stricter limit than login)
        var blockedCtx = CreateHttpContext("/api/v1/auth/register", "10.0.0.2");
        await middleware.InvokeAsync(blockedCtx);

        blockedCtx.Response.StatusCode.Should().Be(429);
    }

    // ─── 429 Response Format ──────────────────────────────────────────────────

    [Fact]
    public async Task BlockedResponse_IncludesRetryAfterHeader()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Exhaust login limit
        for (var i = 0; i < 5; i++)
            await middleware.InvokeAsync(CreateHttpContext("/api/v1/auth/login", "10.0.0.3"));

        var blockedCtx = CreateHttpContext("/api/v1/auth/login", "10.0.0.3");
        await middleware.InvokeAsync(blockedCtx);

        blockedCtx.Response.Headers.ContainsKey("Retry-After").Should().BeTrue();
    }

    [Fact]
    public async Task BlockedResponse_HasCorrectContentType()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        for (var i = 0; i < 5; i++)
            await middleware.InvokeAsync(CreateHttpContext("/api/v1/auth/login", "10.0.0.4"));

        var blockedCtx = CreateHttpContext("/api/v1/auth/login", "10.0.0.4");
        await middleware.InvokeAsync(blockedCtx);

        blockedCtx.Response.ContentType.Should().Contain("application/json");
    }

    // ─── X-Forwarded-For ─────────────────────────────────────────────────────

    [Fact]
    public async Task XForwardedFor_UsedForClientIPIdentification()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        const string proxyIp = "172.16.0.1";
        const string realClientIp = "203.0.113.50";

        // Exhaust limit using X-Forwarded-For from the real client IP
        for (var i = 0; i < 5; i++)
        {
            var ctx = CreateHttpContext("/api/v1/auth/login", proxyIp);
            ctx.Request.Headers["X-Forwarded-For"] = realClientIp;
            await middleware.InvokeAsync(ctx);
        }

        // The same real client IP should now be blocked, even through the same proxy
        var blockedCtx = CreateHttpContext("/api/v1/auth/login", proxyIp);
        blockedCtx.Request.Headers["X-Forwarded-For"] = realClientIp;
        await middleware.InvokeAsync(blockedCtx);

        blockedCtx.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task XForwardedFor_DifferentClientIPs_TrackedSeparately()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        const string proxyIp = "172.16.0.1";

        // Exhaust limit for client A through the proxy
        for (var i = 0; i < 5; i++)
        {
            var ctx = CreateHttpContext("/api/v1/auth/login", proxyIp);
            ctx.Request.Headers["X-Forwarded-For"] = "203.0.113.10";
            await middleware.InvokeAsync(ctx);
        }

        // Client B going through the same proxy should still be allowed
        var allowedCtx = CreateHttpContext("/api/v1/auth/login", proxyIp);
        allowedCtx.Request.Headers["X-Forwarded-For"] = "203.0.113.20";
        await middleware.InvokeAsync(allowedCtx);

        allowedCtx.Response.StatusCode.Should().Be(200);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private RateLimitingMiddleware CreateMiddleware(RequestDelegate next)
        => new(next, _loggerMock.Object);

    private static DefaultHttpContext CreateHttpContext(string path, string ip)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = "POST";
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ip);
        context.Response.Body = new MemoryStream();
        return context;
    }
}
