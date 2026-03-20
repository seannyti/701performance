using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Models;

namespace mperformancepower.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await ApplySchemaUpdatesAsync(db);

        // Seed default hero settings row
        if (!db.HeroSettings.Any())
        {
            db.HeroSettings.Add(new HeroSettings
            {
                HeroType = "none",
                OverlayOpacity = 0.5,
                UpdatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        // Seed default admin user
        if (!db.AppUsers.Any())
        {
            db.AppUsers.Add(new AppUser
            {
                Email = "admin@mperformancepower.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234!"),
                Role = "Admin",
                EmailVerified = true,
                IsActive = true,
            });

            await db.SaveChangesAsync();
        }
    }

    private static async Task ApplySchemaUpdatesAsync(AppDbContext db)
    {
        // Add each column individually; ignore errors if they already exist.
        // MySQL 8.0 image on Docker Hub is 8.0.x and may not support
        // ADD COLUMN IF NOT EXISTS in all minor versions.
        // Create HeroSettings table if it doesn't exist
        try
        {
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS HeroSettings (
                    Id INT NOT NULL AUTO_INCREMENT,
                    HeroType LONGTEXT NOT NULL,
                    YoutubeUrl LONGTEXT NULL,
                    YoutubeStartTime INT NOT NULL DEFAULT 0,
                    VideoPath LONGTEXT NULL,
                    ImagePath LONGTEXT NULL,
                    OverlayOpacity DOUBLE NOT NULL DEFAULT 0.5,
                    UpdatedAt DATETIME(6) NOT NULL,
                    PRIMARY KEY (Id)
                )");
        }
        catch { /* table already exists */ }

        var profileColumns = new[]
        {
            "ALTER TABLE Orders ADD COLUMN TrackingNumber VARCHAR(200) NULL",
            "ALTER TABLE AppUsers ADD COLUMN AvatarPath VARCHAR(500) NULL",
        };
        foreach (var sql in profileColumns)
        {
            try { await db.Database.ExecuteSqlRawAsync(sql); }
            catch { /* Column already exists */ }
        }

        var vehicleColumns = new[]
        {
            "ALTER TABLE Vehicles ADD COLUMN Specs LONGTEXT NOT NULL DEFAULT ('[]')",
        };
        foreach (var sql in vehicleColumns)
        {
            try { await db.Database.ExecuteSqlRawAsync(sql); }
            catch { /* Column already exists */ }
        }

        var columns = new[]
        {
            "ALTER TABLE AppUsers ADD COLUMN EmailVerified TINYINT(1) NOT NULL DEFAULT 1",
            "ALTER TABLE AppUsers ADD COLUMN EmailVerificationToken VARCHAR(200) NULL",
            "ALTER TABLE AppUsers ADD COLUMN EmailVerificationTokenExpiry DATETIME(6) NULL",
            "ALTER TABLE AppUsers ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1",
        };

        foreach (var sql in columns)
        {
            try { await db.Database.ExecuteSqlRawAsync(sql); }
            catch { /* Column already exists — ignore duplicate column error */ }
        }
    }
}
