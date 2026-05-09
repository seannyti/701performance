using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PerformancePower.Api.DTOs;

[Collection("Api")]
public class AuthTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // ── Staff login ──────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginStaff_ValidCredentials_Returns200WithToken()
    {
        var res = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin@performancepower.com", password = "Admin@123!" });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.AccessToken);
        Assert.Equal("admin", body.Role);
        Assert.Equal("admin@performancepower.com", body.Email);
    }

    [Fact]
    public async Task LoginStaff_WrongPassword_Returns401()
    {
        var res = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin@performancepower.com", password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task LoginStaff_UnknownEmail_Returns401()
    {
        var res = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "nobody@example.com", password = "Admin@123!" });

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    // ── Customer registration ─────────────────────────────────────────────────

    [Fact]
    public async Task Register_NewCustomer_Returns200WithCustomerRole()
    {
        var res = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Jane",
            lastName  = "Doe",
            email     = $"jane_{Guid.NewGuid():N}@example.com",
            password  = "Password123!",
            phone     = "555-0100",
        });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.AccessToken);
        Assert.Equal("customer", body.Role);
        Assert.Equal("Jane", body.FirstName);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email   = $"dup_{Guid.NewGuid():N}@example.com";
        var payload = new { firstName = "A", lastName = "B", email, password = "Password123!", phone = (string?)null };

        await _client.PostAsJsonAsync("/api/auth/register", payload);
        var res = await _client.PostAsJsonAsync("/api/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    // ── Customer login ────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginCustomer_ValidCredentials_Returns200WithCustomerRole()
    {
        var email = $"cust_{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register",
            new { firstName = "Test", lastName = "Customer", email, password = "Password123!", phone = (string?)null });

        var res = await _client.PostAsJsonAsync("/api/auth/login/customer",
            new { email, password = "Password123!" });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.Equal("customer", body.Role);
        Assert.NotEmpty(body.AccessToken);
    }

    [Fact]
    public async Task LoginCustomer_WrongPassword_Returns401()
    {
        var email = $"cust_{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register",
            new { firstName = "Test", lastName = "Customer", email, password = "Password123!", phone = (string?)null });

        var res = await _client.PostAsJsonAsync("/api/auth/login/customer",
            new { email, password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    // ── /api/auth/me ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Me_WithValidStaffToken_Returns200WithProfile()
    {
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin@performancepower.com", password = "Admin@123!" });
        var auth = await loginRes.Content.ReadFromJsonAsync<AuthResponse>();

        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        req.Headers.Authorization = new("Bearer", auth!.AccessToken);
        var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.Equal("admin@performancepower.com", body!.Email);
        Assert.Equal("admin", body.Role);
    }

    [Fact]
    public async Task Me_WithValidCustomerToken_Returns200WithCustomerRole()
    {
        var email = $"me_{Guid.NewGuid():N}@example.com";
        var regRes = await _client.PostAsJsonAsync("/api/auth/register",
            new { firstName = "Me", lastName = "Test", email, password = "Password123!", phone = (string?)null });
        var auth = await regRes.Content.ReadFromJsonAsync<AuthResponse>();

        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        req.Headers.Authorization = new("Bearer", auth!.AccessToken);
        var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.Equal("customer", body!.Role);
    }

    [Fact]
    public async Task Me_WithNoToken_Returns401()
    {
        var res = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
