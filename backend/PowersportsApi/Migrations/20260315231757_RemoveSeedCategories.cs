using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9878), new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9879) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt", "Value" },
                values: new object[] { new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9881), new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9882), "" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9884), new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9884) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9885), new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9886) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9887), new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9887) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt", "Value" },
                values: new object[] { new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9889), new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9889), "" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt", "Value" },
                values: new object[] { new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9891), new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9891), "" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9893), new DateTime(2026, 3, 15, 23, 17, 56, 801, DateTimeKind.Utc).AddTicks(9893) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "ImageUrl", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(86), "All-Terrain Vehicles for work and recreation", null, true, "ATV", new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(86) },
                    { 2, new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(88), "Motorcycles designed for off-road terrain", null, true, "Dirtbike", new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(88) },
                    { 3, new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(90), "Utility Task Vehicles for heavy-duty work", null, true, "UTV", new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(90) },
                    { 4, new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(91), "Winter vehicles for snow terrain", null, true, "Snowmobile", new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(91) },
                    { 5, new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(93), "Safety gear and accessories", null, true, "Gear", new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(93) }
                });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(176), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(176) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt", "Value" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(178), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(179), "Your Gateway to Adventure" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(180), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(180) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(182), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(182) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(184), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(184) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt", "Value" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(186), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(186), "Discover Your Next Adventure" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt", "Value" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(188), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(188), "Premium powersports vehicles and gear for every adventure seeker" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(190), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(190) });
        }
    }
}
