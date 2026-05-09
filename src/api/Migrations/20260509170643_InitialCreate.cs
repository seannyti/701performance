using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PerformancePower.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StockNumber = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Vin = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Make = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Model = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Trim = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Condition = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mileage = table.Column<int>(type: "int", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Msrp = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsFeatured = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SpecSheetUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Specs = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SoldAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MfaEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TotpSecret = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VehicleImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ThumbnailUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleImages_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VehicleHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldValues = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewValues = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehicleHistories_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "admin" },
                    { 7, "superadmin" }
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "Key", "Type", "UpdatedAt", "UpdatedByUserId", "Value" },
                values: new object[,]
                {
                    { 1, "hero_type", "string", new DateTime(2026, 5, 9, 17, 6, 42, 733, DateTimeKind.Utc).AddTicks(9869), null, "none" },
                    { 2, "hero_youtube_url", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(403), null, "" },
                    { 3, "hero_start_time", "int", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(404), null, "0" },
                    { 4, "hero_overlay_opacity", "int", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(405), null, "50" },
                    { 5, "hero_title", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(405), null, "Your Powersports Destination" },
                    { 6, "hero_title_accent", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(406), null, "Destination" },
                    { 7, "hero_subtitle", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(407), null, "ATVs, UTVs, Dirt Bikes, Snowmobiles & More" },
                    { 8, "hero_btn1_label", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(408), null, "Browse Inventory" },
                    { 9, "hero_btn1_link", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(409), null, "/inventory" },
                    { 10, "hero_btn2_label", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(410), null, "Contact Us" },
                    { 11, "hero_btn2_link", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(410), null, "/contact" },
                    { 12, "contact_phone", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(411), null, "" },
                    { 13, "contact_email", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(412), null, "" },
                    { 14, "contact_address", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(412), null, "" },
                    { 15, "business_hours", "json", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(413), null, "{}" },
                    { 16, "social_facebook", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(423), null, "" },
                    { 17, "social_instagram", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(424), null, "" },
                    { 18, "social_youtube", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(424), null, "" },
                    { 19, "seo_title", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(425), null, "PerformancePower Powersports" },
                    { 20, "seo_description", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(426), null, "ATVs, UTVs, Dirt Bikes, Snowmobiles & More" },
                    { 21, "announcement_enabled", "bool", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(426), null, "false" },
                    { 22, "announcement_text", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(427), null, "" },
                    { 23, "about_content", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(428), null, "" },
                    { 24, "theme_primary_color", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(428), null, "#e53935" },
                    { 25, "theme_logo_url", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(429), null, "" },
                    { 26, "octane_embed_url", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(430), null, "" },
                    { 27, "synchrony_url", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(430), null, "" },
                    { 28, "lender_synchrony_description", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(431), null, "Synchrony offers competitive rates and flexible terms for powersports financing." },
                    { 29, "lender_octane_description", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(431), null, "Octane specializes in powersports lending with a quick pre-qualification." },
                    { 32, "maps_embed_url", "string", new DateTime(2026, 5, 9, 17, 6, 42, 734, DateTimeKind.Utc).AddTicks(432), null, "" }
                });

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
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleHistories_UserId",
                table: "VehicleHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleHistories_VehicleId",
                table: "VehicleHistories",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleImages_VehicleId",
                table: "VehicleImages",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_StockNumber",
                table: "Vehicles",
                column: "StockNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Vin",
                table: "Vehicles",
                column: "Vin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "VehicleHistories");

            migrationBuilder.DropTable(
                name: "VehicleImages");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
