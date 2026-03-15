using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "MediaFiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MediaSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaSections_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4206), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4207) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4208), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4209) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4210), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4210) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4212), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4212) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4213), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4213) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4296), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4296) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4298), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4299) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4300), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4301) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4302), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4303) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4304), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4305) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4306), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4306) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4308), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4308) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4310), new DateTime(2026, 3, 10, 23, 40, 37, 677, DateTimeKind.Utc).AddTicks(4310) });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_SectionId",
                table: "MediaFiles",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaSections_CategoryId",
                table: "MediaSections",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaSections_Name",
                table: "MediaSections",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_MediaSections_SectionId",
                table: "MediaFiles",
                column: "SectionId",
                principalTable: "MediaSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_MediaSections_SectionId",
                table: "MediaFiles");

            migrationBuilder.DropTable(
                name: "MediaSections");

            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_SectionId",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "MediaFiles");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8744), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8744) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8746), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8746) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8748), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8748) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8749), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8749) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8751), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8751) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8832), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8832) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8835), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8835) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8837), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8837) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8839), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8839) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8841), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8841) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8845), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8845) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8848), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8848) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8852), new DateTime(2026, 3, 3, 8, 9, 26, 233, DateTimeKind.Utc).AddTicks(8852) });
        }
    }
}
