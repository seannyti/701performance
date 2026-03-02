using Microsoft.EntityFrameworkCore;
using PowersportsApi.Models;

namespace PowersportsApi.Data;

/// <summary>
/// Database context for the Powersports application
/// Supports both in-memory and SQL Server databases
/// </summary>
public class PowersportsDbContext : DbContext
{
    public PowersportsDbContext(DbContextOptions<PowersportsDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<ProductImage> ProductImages { get; set; } = null!;
    public DbSet<CategoryImage> CategoryImages { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<SiteSetting> SiteSettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure relationship with Category
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure ProductImage entity
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.OriginalName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ThumbnailPath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure relationship with Product
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.ProductImages)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => new { e.ProductId, e.IsMain });
        });

        // Configure CategoryImage entity
        modelBuilder.Entity<CategoryImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.OriginalName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ThumbnailPath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure relationship with Category (one-to-one)
            entity.HasOne(e => e.Category)
                  .WithOne(c => c.CategoryImage)
                  .HasForeignKey<CategoryImage>(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.CategoryId).IsUnique();
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ExpiryDate).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure relationship with User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
        });

        // Configure SiteSetting entity
        modelBuilder.Entity<SiteSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Unique key constraint
            entity.HasIndex(e => e.Key).IsUnique();
            
            // Configure relationship with User (LastModifiedBy)
            entity.HasOne(e => e.LastModifiedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.LastModifiedBy)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categories first
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = 1,
                Name = "ATV",
                Description = "All-Terrain Vehicles for work and recreation",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Category
            {
                Id = 2,
                Name = "Dirtbike",
                Description = "Motorcycles designed for off-road terrain",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Category
            {
                Id = 3,
                Name = "UTV",
                Description = "Utility Task Vehicles for heavy-duty work",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Category
            {
                Id = 4,
                Name = "Snowmobile",
                Description = "Winter vehicles for snow terrain",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Category
            {
                Id = 5,
                Name = "Gear",
                Description = "Safety gear and accessories",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed Site Settings for dynamic content management
        modelBuilder.Entity<SiteSetting>().HasData(
            new SiteSetting
            {
                Id = 1,
                Key = "site_name",
                DisplayName = "Company Name",
                Value = "701 Performance Power",
                Description = "The main company name displayed throughout the website",
                Type = SettingType.Text,
                Category = "General",
                SortOrder = 1,
                IsRequired = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 2,
                Key = "site_tagline",
                DisplayName = "Company Tagline",
                Value = "Your Gateway to Adventure",
                Description = "Brief tagline or motto displayed in headers and hero sections",
                Type = SettingType.Text,
                Category = "General",
                SortOrder = 2,
                IsRequired = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 3,
                Key = "contact_email",
                DisplayName = "Contact Email",
                Value = "info@701performancepower.com",
                Description = "Main contact email address",
                Type = SettingType.Email,
                Category = "General",
                SortOrder = 3,
                IsRequired = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 4,
                Key = "contact_phone",
                DisplayName = "Contact Phone",
                Value = "",
                Description = "Primary phone number for customer contact",
                Type = SettingType.Phone,
                Category = "General",
                SortOrder = 4,
                IsRequired = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 5,
                Key = "contact_address",
                DisplayName = "Business Address",
                Value = "",
                Description = "Physical business address",
                Type = SettingType.TextArea,
                Category = "General",
                SortOrder = 5,
                IsRequired = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 6,
                Key = "hero_title",
                DisplayName = "Homepage Hero Title",
                Value = "Discover Your Next Adventure",
                Description = "Main heading on the homepage hero section",
                Type = SettingType.Text,
                Category = "General",
                SortOrder = 6,
                IsRequired = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 7,
                Key = "hero_subtitle",
                DisplayName = "Homepage Hero Subtitle",
                Value = "Premium powersports vehicles and gear for every adventure seeker",
                Description = "Subtitle text below the hero title",
                Type = SettingType.TextArea,
                Category = "General",
                SortOrder = 7,
                IsRequired = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 8,
                Key = "allow_user_registration",
                DisplayName = "Allow User Registration",
                Value = "true",
                Description = "Whether new users can register on the site",
                Type = SettingType.Boolean,
                Category = "Security",
                SortOrder = 1,
                IsRequired = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}