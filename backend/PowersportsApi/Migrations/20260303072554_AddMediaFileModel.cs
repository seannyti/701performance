using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaFileModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MediaType = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    AltText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaFiles_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(26), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(26) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(28), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(28) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(29), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(30) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(31), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(31) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(32), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(32) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(109), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(109) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(112), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(112) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(114), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(115) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(116), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(116) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(118), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(118) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(120), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(120) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(122), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(122) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(124), new DateTime(2026, 3, 3, 7, 25, 54, 572, DateTimeKind.Utc).AddTicks(124) });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_MediaType",
                table: "MediaFiles",
                column: "MediaType");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_UploadedAt",
                table: "MediaFiles",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_UploadedByUserId",
                table: "MediaFiles",
                column: "UploadedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8868), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8868) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8870), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8870) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8872), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8872) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8873), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8874) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8875), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8875) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8976), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8976) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8980), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8981) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8983), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8983) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8985), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8985) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8986), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8987) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8988), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8989) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8990), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8991) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8992), new DateTime(2026, 3, 3, 5, 43, 9, 212, DateTimeKind.Utc).AddTicks(8992) });
        }
    }
}
