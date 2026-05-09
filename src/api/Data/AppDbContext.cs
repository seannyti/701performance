using Microsoft.EntityFrameworkCore;
using PerformancePower.Api.Models;

namespace PerformancePower.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Auth & Staff
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Inventory
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleImage> VehicleImages => Set<VehicleImage>();
    public DbSet<VehicleHistory> VehicleHistories => Set<VehicleHistory>();

    // Settings
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "admin" },
            new Role { Id = 7, Name = "superadmin" }
        );

        // Seed default site settings
        modelBuilder.Entity<SiteSetting>().HasData(
            new SiteSetting { Id = 1, Key = "hero_type", Value = "none", Type = "string" },
            new SiteSetting { Id = 2, Key = "hero_youtube_url", Value = "", Type = "string" },
            new SiteSetting { Id = 3, Key = "hero_start_time", Value = "0", Type = "int" },
            new SiteSetting { Id = 4, Key = "hero_overlay_opacity", Value = "50", Type = "int" },
            new SiteSetting { Id = 5, Key = "hero_title", Value = "Your Powersports Destination", Type = "string" },
            new SiteSetting { Id = 6, Key = "hero_title_accent", Value = "Destination", Type = "string" },
            new SiteSetting { Id = 7, Key = "hero_subtitle", Value = "ATVs, UTVs, Dirt Bikes, Snowmobiles & More", Type = "string" },
            new SiteSetting { Id = 8, Key = "hero_btn1_label", Value = "Browse Inventory", Type = "string" },
            new SiteSetting { Id = 9, Key = "hero_btn1_link", Value = "/inventory", Type = "string" },
            new SiteSetting { Id = 10, Key = "hero_btn2_label", Value = "Contact Us", Type = "string" },
            new SiteSetting { Id = 11, Key = "hero_btn2_link", Value = "/contact", Type = "string" },
            new SiteSetting { Id = 12, Key = "contact_phone", Value = "", Type = "string" },
            new SiteSetting { Id = 13, Key = "contact_email", Value = "", Type = "string" },
            new SiteSetting { Id = 14, Key = "contact_address", Value = "", Type = "string" },
            new SiteSetting { Id = 15, Key = "business_hours", Value = "{}", Type = "json" },
            new SiteSetting { Id = 16, Key = "social_facebook", Value = "", Type = "string" },
            new SiteSetting { Id = 17, Key = "social_instagram", Value = "", Type = "string" },
            new SiteSetting { Id = 18, Key = "social_youtube", Value = "", Type = "string" },
            new SiteSetting { Id = 19, Key = "seo_title", Value = "PerformancePower Powersports", Type = "string" },
            new SiteSetting { Id = 20, Key = "seo_description", Value = "ATVs, UTVs, Dirt Bikes, Snowmobiles & More", Type = "string" },
            new SiteSetting { Id = 21, Key = "announcement_enabled", Value = "false", Type = "bool" },
            new SiteSetting { Id = 22, Key = "announcement_text", Value = "", Type = "string" },
            new SiteSetting { Id = 23, Key = "about_content", Value = "", Type = "string" },
            new SiteSetting { Id = 24, Key = "theme_primary_color", Value = "#e53935", Type = "string" },
            new SiteSetting { Id = 25, Key = "theme_logo_url", Value = "", Type = "string" },
            new SiteSetting { Id = 26, Key = "octane_embed_url", Value = "", Type = "string" },
            new SiteSetting { Id = 27, Key = "synchrony_url", Value = "", Type = "string" },
            new SiteSetting { Id = 28, Key = "lender_synchrony_description", Value = "Synchrony offers competitive rates and flexible terms for powersports financing.", Type = "string" },
            new SiteSetting { Id = 29, Key = "lender_octane_description", Value = "Octane specializes in powersports lending with a quick pre-qualification.", Type = "string" },
            new SiteSetting { Id = 32, Key = "maps_embed_url", Value = "", Type = "string" }
        );

        // Indexes
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.Vin);
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.StockNumber).IsUnique();
        modelBuilder.Entity<SiteSetting>().HasIndex(s => s.Key).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(r => r.Token).IsUnique();
    }
}
