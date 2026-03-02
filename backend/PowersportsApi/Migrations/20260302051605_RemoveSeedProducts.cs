using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4291), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4292) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4293), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4294) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4295), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4295) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4297), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4297) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4298), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4298) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4379), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4379) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4381), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4382) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4384), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4384) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4385), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4386) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4387), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4388) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4389), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4390) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4391), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4391) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4393), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4393) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9451), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9452) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9453), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9454) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9455), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9455) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9457), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9457) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9458), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9458) });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImageUrl", "IsActive", "IsFeatured", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9556), "Powerful 4WD ATV perfect for trail riding and utility work. Features a 686cc engine with reliable Ultramatic transmission.", "https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=400&h=300&fit=crop", true, false, "Yamaha Grizzly 700", 12999.99m, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9557) },
                    { 2, 2, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9559), "Championship-winning motocross bike with advanced suspension and lightweight aluminum frame. Built for competition.", "https://images.unsplash.com/photo-1558577452-838b5e2b6b75?w=400&h=300&fit=crop", true, false, "Honda CRF450R", 9899.99m, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9559) },
                    { 3, 3, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9561), "High-performance side-by-side UTV with sport suspension and powerful ProStar engine. Ready for extreme terrain.", "https://images.unsplash.com/photo-1571068316344-75bc76f77890?w=400&h=300&fit=crop", true, false, "Polaris RZR XP 1000", 21999.99m, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9561) },
                    { 4, 4, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9563), "Premium racing snowmobile with 850 E-TEC engine and advanced RAS X suspension for precise handling on trails.", "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=400&h=300&fit=crop", true, false, "Ski-Doo MXZ X-RS", 16499.99m, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9564) },
                    { 5, 5, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9565), "Professional-grade motocross helmet with MIPS technology and superior ventilation. DOT and ECE certified.", "https://images.unsplash.com/photo-1558618542-b3d9f7c05de4?w=400&h=300&fit=crop", true, false, "Fox Racing V3 Helmet", 649.99m, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9565) },
                    { 6, 3, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9567), "Turbocharged side-by-side with industry-leading power and innovative Intelligent Throttle Control.", "https://images.unsplash.com/photo-1591737622611-b6c5ae78542d?w=400&h=300&fit=crop", true, false, "Can-Am Maverick X3", 24999.99m, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9567) },
                    { 7, 2, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9569), "Lightweight 4-stroke motocross bike with championship-proven performance and advanced WP XACT suspension.", "https://images.unsplash.com/photo-1606768666853-403c90a981ad?w=400&h=300&fit=crop", true, false, "KTM 250 SX-F", 8999.99m, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9570) },
                    { 8, 4, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9571), "Mountain snowmobile with revolutionary Alpha rear suspension and powerful 800 H.O. engine for deep snow performance.", "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=400&h=300&fit=crop", true, false, "Arctic Cat Alpha One", 15299.99m, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9571) },
                    { 9, 5, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9573), "Premium motocross boots with advanced protection system and lightweight microfiber construction.", "https://images.unsplash.com/photo-1544966503-7cc5ac882d5b?w=400&h=300&fit=crop", true, false, "Alpinestars Tech 10 Boots", 599.99m, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9573) },
                    { 10, 1, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9575), "Versatile utility ATV with electric power steering and independent rear suspension. Perfect for work and recreation.", "https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=400&h=300&fit=crop", true, false, "Honda Foreman 520", 8999.99m, new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9575) }
                });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9598), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9598) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9601), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9602) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9603), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9604) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9606), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9606) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9609), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9609) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9611), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9611) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9612), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9613) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9614), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9615) });
        }
    }
}
