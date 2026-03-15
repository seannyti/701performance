using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSpecifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Specifications",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2494), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2494) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2496), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2496) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2497), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2498) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2499), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2499) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2500), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2501) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2589), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2589) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2591), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2592) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2593), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2594) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2595), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2596) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2597), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2597) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2599), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2599) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2601), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2601) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2603), new DateTime(2026, 3, 11, 13, 45, 15, 803, DateTimeKind.Utc).AddTicks(2603) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specifications",
                table: "Products");

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
        }
    }
}
