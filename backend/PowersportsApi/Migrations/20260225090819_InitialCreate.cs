using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SubscribeNewsletter = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OriginalName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryImages_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteSettings_Users_LastModifiedBy",
                        column: x => x.LastModifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OriginalName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "ImageUrl", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8431), "All-Terrain Vehicles for work and recreation", null, true, "ATV", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8431) },
                    { 2, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8433), "Motorcycles designed for off-road terrain", null, true, "Dirtbike", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8433) },
                    { 3, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8436), "Utility Task Vehicles for heavy-duty work", null, true, "UTV", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8436) },
                    { 4, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8439), "Winter vehicles for snow terrain", null, true, "Snowmobile", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8440) },
                    { 5, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8441), "Safety gear and accessories", null, true, "Gear", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8442) }
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "DisplayName", "IsActive", "IsRequired", "Key", "LastModifiedBy", "SortOrder", "Type", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, "Company Info", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8565), "The main company name displayed throughout the website", "Company Name", true, true, "company.name", null, 1, 0, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8565), "PowerSports Showcase" },
                    { 2, "Company Info", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8570), "Brief tagline or motto displayed in headers and hero sections", "Company Tagline", true, false, "company.tagline", null, 2, 0, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8571), "Your Gateway to Adventure" },
                    { 3, "Contact Info", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8572), "Main contact email address", "Contact Email", true, true, "contact.email", null, 1, 3, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8573), "info@powersportsshowcase.com" },
                    { 4, "Contact Info", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8575), "Primary phone number for customer contact", "Contact Phone", true, true, "contact.phone", null, 2, 4, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8576), "(555) 123-4567" },
                    { 5, "Contact Info", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8578), "Physical business address", "Business Address", true, false, "contact.address", null, 3, 1, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8578), "123 Adventure Lane, Outdoor City, OC 12345" },
                    { 6, "Footer", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8581), "Copyright notice displayed in the footer", "Copyright Text", true, true, "footer.copyright", null, 1, 0, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8581), "© 2026 PowerSports Showcase. All rights reserved." },
                    { 7, "Homepage", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8584), "Main heading on the homepage hero section", "Homepage Hero Title", true, true, "homepage.hero.title", null, 1, 0, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8584), "Discover Your Next Adventure" },
                    { 8, "Homepage", new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8586), "Subtitle text below the hero title", "Homepage Hero Subtitle", true, false, "homepage.hero.subtitle", null, 2, 1, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8586), "Premium powersports vehicles and gear for every adventure seeker" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImageUrl", "IsActive", "IsFeatured", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8525), "Powerful 4WD ATV perfect for trail riding and utility work. Features a 686cc engine with reliable Ultramatic transmission.", "https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=400&h=300&fit=crop", true, false, "Yamaha Grizzly 700", 12999.99m, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8526) },
                    { 2, 2, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8529), "Championship-winning motocross bike with advanced suspension and lightweight aluminum frame. Built for competition.", "https://images.unsplash.com/photo-1558577452-838b5e2b6b75?w=400&h=300&fit=crop", true, false, "Honda CRF450R", 9899.99m, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8529) },
                    { 3, 3, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8531), "High-performance side-by-side UTV with sport suspension and powerful ProStar engine. Ready for extreme terrain.", "https://images.unsplash.com/photo-1571068316344-75bc76f77890?w=400&h=300&fit=crop", true, false, "Polaris RZR XP 1000", 21999.99m, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8531) },
                    { 4, 4, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8532), "Premium racing snowmobile with 850 E-TEC engine and advanced RAS X suspension for precise handling on trails.", "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=400&h=300&fit=crop", true, false, "Ski-Doo MXZ X-RS", 16499.99m, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8533) },
                    { 5, 5, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8534), "Professional-grade motocross helmet with MIPS technology and superior ventilation. DOT and ECE certified.", "https://images.unsplash.com/photo-1558618542-b3d9f7c05de4?w=400&h=300&fit=crop", true, false, "Fox Racing V3 Helmet", 649.99m, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8535) },
                    { 6, 3, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8537), "Turbocharged side-by-side with industry-leading power and innovative Intelligent Throttle Control.", "https://images.unsplash.com/photo-1591737622611-b6c5ae78542d?w=400&h=300&fit=crop", true, false, "Can-Am Maverick X3", 24999.99m, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8537) },
                    { 7, 2, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8539), "Lightweight 4-stroke motocross bike with championship-proven performance and advanced WP XACT suspension.", "https://images.unsplash.com/photo-1606768666853-403c90a981ad?w=400&h=300&fit=crop", true, false, "KTM 250 SX-F", 8999.99m, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8539) },
                    { 8, 4, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8540), "Mountain snowmobile with revolutionary Alpha rear suspension and powerful 800 H.O. engine for deep snow performance.", "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=400&h=300&fit=crop", true, false, "Arctic Cat Alpha One", 15299.99m, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8541) },
                    { 9, 5, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8542), "Premium motocross boots with advanced protection system and lightweight microfiber construction.", "https://images.unsplash.com/photo-1544966503-7cc5ac882d5b?w=400&h=300&fit=crop", true, false, "Alpinestars Tech 10 Boots", 599.99m, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8542) },
                    { 10, 1, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8544), "Versatile utility ATV with electric power steering and independent rear suspension. Perfect for work and recreation.", "https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=400&h=300&fit=crop", true, false, "Honda Foreman 520", 8999.99m, new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8544) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryImages_CategoryId",
                table: "CategoryImages",
                column: "CategoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_IsMain",
                table: "ProductImages",
                columns: new[] { "ProductId", "IsMain" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_Key",
                table: "SiteSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_LastModifiedBy",
                table: "SiteSettings",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryImages");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
