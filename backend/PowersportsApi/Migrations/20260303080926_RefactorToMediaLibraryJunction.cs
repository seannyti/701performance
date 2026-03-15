using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToMediaLibraryJunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "OriginalName",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "CategoryImages");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "CategoryImages");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "CategoryImages");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "CategoryImages");

            migrationBuilder.DropColumn(
                name: "OriginalName",
                table: "CategoryImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "CategoryImages");

            migrationBuilder.AddColumn<int>(
                name: "MediaFileId",
                table: "ProductImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MediaFileId",
                table: "CategoryImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_MediaFileId",
                table: "ProductImages",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_SortOrder",
                table: "ProductImages",
                columns: new[] { "ProductId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryImages_MediaFileId",
                table: "CategoryImages",
                column: "MediaFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryImages_MediaFiles_MediaFileId",
                table: "CategoryImages",
                column: "MediaFileId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_MediaFiles_MediaFileId",
                table: "ProductImages",
                column: "MediaFileId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryImages_MediaFiles_MediaFileId",
                table: "CategoryImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_MediaFiles_MediaFileId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_MediaFileId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_SortOrder",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_CategoryImages_MediaFileId",
                table: "CategoryImages");

            migrationBuilder.DropColumn(
                name: "MediaFileId",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "MediaFileId",
                table: "CategoryImages");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "ProductImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "ProductImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "ProductImages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "ProductImages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                table: "ProductImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "ProductImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "CategoryImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "CategoryImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "CategoryImages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "CategoryImages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                table: "CategoryImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "CategoryImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

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
        }
    }
}
