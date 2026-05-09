using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PerformancePower.Api.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Design-time factory used by 'dotnet ef' migration tooling only.
        // Set DB_CONNECTION_STRING env var, or it falls back to the local dev default.
        var cs = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Server=localhost;Port=3306;Database=performancepower;User=ppuser;Password=pppassword;";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySql(cs, ServerVersion.Parse("8.0.0-mysql"));
        return new AppDbContext(optionsBuilder.Options);
    }
}
