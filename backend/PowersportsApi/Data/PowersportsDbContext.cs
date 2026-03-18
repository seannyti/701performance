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
    public DbSet<ContactSubmission> ContactSubmissions { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<MediaFile> MediaFiles { get; set; } = null!;
    public DbSet<MediaSection> MediaSections { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<ChatSession> ChatSessions { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

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
            entity.Property(e => e.Specifications).HasColumnType("longtext");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
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
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure ProductImage entity (junction table with MediaFile)
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MediaFileId).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            // Configure relationship with Product
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.ProductImages)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Configure relationship with MediaFile
            entity.HasOne(e => e.MediaFile)
                  .WithMany()
                  .HasForeignKey(e => e.MediaFileId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.MediaFileId);
            entity.HasIndex(e => new { e.ProductId, e.IsMain });
            entity.HasIndex(e => new { e.ProductId, e.SortOrder });
        });

        // Configure CategoryImage entity (junction table with MediaFile)
        modelBuilder.Entity<CategoryImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MediaFileId).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            // Configure relationship with Category (one-to-one)
            entity.HasOne(e => e.Category)
                  .WithOne(c => c.CategoryImage)
                  .HasForeignKey<CategoryImage>(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Configure relationship with MediaFile
            entity.HasOne(e => e.MediaFile)
                  .WithMany()
                  .HasForeignKey(e => e.MediaFileId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.CategoryId).IsUnique();
            entity.HasIndex(e => e.MediaFileId);
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
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ExpiryDate).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
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
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            // Unique key constraint
            entity.HasIndex(e => e.Key).IsUnique();
            
            // Configure relationship with User (LastModifiedBy)
            entity.HasOne(e => e.LastModifiedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.LastModifiedBy)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ContactSubmission entity
        modelBuilder.Entity<ContactSubmission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.AdminNotes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            // Configure relationship with assigned user
            entity.HasOne(e => e.AssignedToUser)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedToUserId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            // Index for status queries
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ShippingCity).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ShippingState).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShippingZipCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ShippingCountry).HasMaxLength(100);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShippingCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);
            entity.Property(e => e.ShippingCarrier).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            // Configure relationship with User
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            // Unique order number
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            
            // Indexes for queries
            entity.HasIndex(e => e.OrderStatus);
            entity.HasIndex(e => e.PaymentStatus);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ProductSku).HasMaxLength(100);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            
            // Configure relationship with Order
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Configure relationship with Product
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure MediaFile entity
        modelBuilder.Entity<MediaFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ThumbnailPath).HasMaxLength(1000);
            entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AltText).HasMaxLength(500);
            entity.Property(e => e.Caption).HasMaxLength(1000);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.MediaType).HasConversion<int>();
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            // Configure relationship with User
            entity.HasOne(e => e.UploadedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UploadedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            // Configure relationship with MediaSection
            entity.HasOne(e => e.Section)
                  .WithMany(s => s.MediaFiles)
                  .HasForeignKey(e => e.SectionId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.MediaType);
            entity.HasIndex(e => e.SectionId);
            entity.HasIndex(e => e.UploadedAt);
        });

        // Configure MediaSection entity
        modelBuilder.Entity<MediaSection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            // Configure relationship with Category (optional)
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.CategoryId);
        });

        // Configure ChatSession entity
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GuestName).HasMaxLength(100);
            entity.Property(e => e.GuestEmail).HasMaxLength(200);
            entity.Property(e => e.AgentConnectionId).HasMaxLength(200);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Messages)
                  .WithOne(m => m.Session)
                  .HasForeignKey(m => m.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure ChatMessage entity
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SenderName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SenderRole).HasConversion<int>();
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.SentAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.SentAt);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
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
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 2,
                Key = "site_tagline",
                DisplayName = "Company Tagline",
                Value = "",
                Description = "Brief tagline or motto displayed in headers and hero sections",
                Type = SettingType.Text,
                Category = "General",
                SortOrder = 2,
                IsRequired = false,
                IsActive = true,
                IsPublic = true,
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
                IsPublic = true,
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
                IsPublic = true,
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
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 6,
                Key = "hero_title",
                DisplayName = "Homepage Hero Title",
                Value = "",
                Description = "Main heading on the homepage hero section",
                Type = SettingType.Text,
                Category = "General",
                SortOrder = 6,
                IsRequired = false,
                IsActive = true,
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 7,
                Key = "hero_subtitle",
                DisplayName = "Homepage Hero Subtitle",
                Value = "",
                Description = "Subtitle text below the hero title",
                Type = SettingType.TextArea,
                Category = "General",
                SortOrder = 7,
                IsRequired = false,
                IsActive = true,
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 9,
                Key = "hero_video_enabled",
                DisplayName = "Enable Hero Video Background",
                Value = "false",
                Description = "Show a video instead of a static image in the hero section",
                Type = SettingType.Boolean,
                Category = "Content",
                SortOrder = 8,
                IsRequired = false,
                IsActive = true,
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 10,
                Key = "hero_video_url",
                DisplayName = "Hero Video URL",
                Value = "",
                Description = "YouTube URL or uploaded MP4 path for the hero background video",
                Type = SettingType.Url,
                Category = "Content",
                SortOrder = 9,
                IsRequired = false,
                IsActive = true,
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SiteSetting
            {
                Id = 11,
                Key = "hero_video_start",
                DisplayName = "Hero Video Start Time",
                Value = "0",
                Description = "Number of seconds to skip at the start of the video",
                Type = SettingType.Number,
                Category = "Content",
                SortOrder = 10,
                IsRequired = false,
                IsActive = true,
                IsPublic = true,
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
                IsPublic = false,   // auth config — never expose publicly
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}