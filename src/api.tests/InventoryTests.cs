using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PerformancePower.Api.DTOs;

[Collection("Api")]
public class InventoryTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> AdminTokenAsync()
    {
        var res = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin@performancepower.com", password = "Admin@123!" });
        var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.AccessToken;
    }

    private HttpRequestMessage Authed(HttpMethod method, string url, string token)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    private static object NewVehicle(string make = "Polaris", string model = "RZR", string type = "utv") => new
    {
        year      = 2024,
        make,
        model,
        type,
        condition = "new",
        cost      = 18_000m,
        msrp      = 26_999m,
        salePrice = 25_999m,
        status    = "available",
    };

    // ── Public list endpoint ─────────────────────────────────────────────────

    [Fact]
    public async Task GetInventory_Anonymous_Returns200()
    {
        var res = await _client.GetAsync("/api/inventory");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task GetInventory_ReturnsPagedResult()
    {
        var res  = await _client.GetAsync("/api/inventory?page=1&pageSize=5");
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.True(body.TryGetProperty("total", out _));
        Assert.True(body.TryGetProperty("data",  out _));
    }

    // ── Create vehicle ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateVehicle_Authenticated_Returns201WithStockNumber()
    {
        var token = await AdminTokenAsync();
        var req   = Authed(HttpMethod.Post, "/api/inventory", token);
        req.Content = JsonContent.Create(NewVehicle());

        var res  = await _client.SendAsync(req);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var stock = body.GetProperty("stockNumber").GetString()!;
        Assert.StartsWith("PP", stock);
        Assert.Equal(8, stock.Length); // PP + 2-digit year + 4-digit seq
    }

    [Fact]
    public async Task CreateVehicle_Anonymous_Returns401()
    {
        var res = await _client.PostAsJsonAsync("/api/inventory", NewVehicle());
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    // ── Get by ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetVehicleById_NotFound_Returns404()
    {
        var res = await _client.GetAsync("/api/inventory/999999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task GetVehicleById_ExistingVehicle_Returns200WithDetails()
    {
        var token = await AdminTokenAsync();
        var createReq = Authed(HttpMethod.Post, "/api/inventory", token);
        createReq.Content = JsonContent.Create(NewVehicle("Can-Am", "Maverick X3", "utv"));
        var createRes = await _client.SendAsync(createReq);
        var created   = await createRes.Content.ReadFromJsonAsync<JsonElement>();
        var id        = created.GetProperty("id").GetInt32();

        var res  = await _client.GetAsync($"/api/inventory/{id}");
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("Can-Am", body.GetProperty("make").GetString());
        Assert.Equal("Maverick X3", body.GetProperty("model").GetString());
    }

    // ── Update vehicle ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateVehicle_Authenticated_Returns200()
    {
        var token = await AdminTokenAsync();

        // Create
        var createReq = Authed(HttpMethod.Post, "/api/inventory", token);
        createReq.Content = JsonContent.Create(NewVehicle("KTM", "450 SX-F", "dirtbike"));
        var createRes = await _client.SendAsync(createReq);
        var created   = await createRes.Content.ReadFromJsonAsync<JsonElement>();
        var id        = created.GetProperty("id").GetInt32();

        // Update — endpoint requires all fields (same shape as create)
        var updateReq = Authed(HttpMethod.Put, $"/api/inventory/{id}", token);
        updateReq.Content = JsonContent.Create(new
        {
            year = 2024, make = "KTM", model = "450 SX-F", type = "dirtbike",
            condition = "new", cost = 18_000m, msrp = 26_999m, salePrice = 8_999m, status = "available",
        });
        var res = await _client.SendAsync(updateReq);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    // ── Delete vehicle ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteVehicle_AsAdmin_Returns204()
    {
        var token = await AdminTokenAsync();

        var createReq = Authed(HttpMethod.Post, "/api/inventory", token);
        createReq.Content = JsonContent.Create(NewVehicle("Yamaha", "YZ250F", "dirtbike"));
        var createRes = await _client.SendAsync(createReq);
        var created   = await createRes.Content.ReadFromJsonAsync<JsonElement>();
        var id        = created.GetProperty("id").GetInt32();

        var deleteReq = Authed(HttpMethod.Delete, $"/api/inventory/{id}", token);
        var res       = await _client.SendAsync(deleteReq);

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
    }

    [Fact]
    public async Task DeleteVehicle_Anonymous_Returns401()
    {
        var res = await _client.DeleteAsync("/api/inventory/1");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
