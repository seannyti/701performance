using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Shared test server factory. Uses SQLite in-memory with a persistent connection
/// so all requests within a test run share the same database state.
/// The "Testing" environment branch in Program.cs registers the SQLite DbContext
/// and calls EnsureCreated() instead of Migrate().
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    // Keep this connection open for the lifetime of the factory so the
    // named in-memory SQLite database (testdb) is not garbage-collected.
    private readonly SqliteConnection _keepAlive =
        new("DataSource=testdb;Mode=Memory;Cache=Shared");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" environment causes Program.cs to:
        //   - skip the MySQL DbContext registration (we provide SQLite below)
        //   - call EnsureCreated() instead of MigrateAsync()
        builder.UseEnvironment("Testing");

        _keepAlive.Open();

        builder.ConfigureServices(services =>
        {
            // Program.cs skips DbContext registration in Testing — we add SQLite here.
            // The open _keepAlive connection keeps the named in-memory DB alive
            // for the full duration of the test run.
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_keepAlive));
        });

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]         = "test-secret-key-must-be-at-least-32-chars!!",
                ["Jwt:Issuer"]         = "test-issuer",
                ["Jwt:Audience"]       = "test-audience",
                ["Jwt:ExpiryMinutes"]  = "60",
                ["Jwt:RefreshDays"]    = "7",
                ["Admin:SeedPassword"] = "Admin@123!",
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _keepAlive.Dispose();
    }
}
