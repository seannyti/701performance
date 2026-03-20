using Microsoft.EntityFrameworkCore;
using mperformancepower.Api.Models;

namespace mperformancepower.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleImage> VehicleImages => Set<VehicleImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<HeroSettings> HeroSettings => Set<HeroSettings>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Vehicle>()
            .Property(v => v.Price).HasColumnType("decimal(10,2)");

        b.Entity<Vehicle>()
            .Property(v => v.Condition).HasConversion<string>();

        b.Entity<Vehicle>()
            .HasOne(v => v.Category)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<VehicleImage>()
            .HasIndex(i => new { i.VehicleId, i.DisplayOrder });

        b.Entity<Order>()
            .Property(o => o.SalePrice).HasColumnType("decimal(10,2)");
        b.Entity<Order>()
            .Property(o => o.DownPayment).HasColumnType("decimal(10,2)");
        b.Entity<Order>()
            .Property(o => o.LoanAmount).HasColumnType("decimal(10,2)");
        b.Entity<Order>()
            .Property(o => o.APR).HasColumnType("decimal(5,3)");

        b.Entity<Order>()
            .HasOne(o => o.Vehicle)
            .WithMany()
            .HasForeignKey(o => o.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Order>()
            .HasOne(o => o.Inquiry)
            .WithMany()
            .HasForeignKey(o => o.InquiryId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<AppUser>()
            .HasIndex(u => u.Email).IsUnique();

        b.Entity<Appointment>()
            .HasOne(a => a.Vehicle)
            .WithMany()
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<Appointment>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<ChatMessage>()
            .HasOne(m => m.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ChatSession>()
            .Property(s => s.Id).HasMaxLength(64);
        b.Entity<ChatMessage>()
            .Property(m => m.SessionId).HasMaxLength(64);

        b.Entity<SiteSetting>().HasKey(s => s.Section);
    }
}
