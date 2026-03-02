using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LowStockThreshold",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9556), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9557) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9559), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9559) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9561), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9561) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9563), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9564) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9565), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9565) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9567), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9567) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9569), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9570) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9571), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9571) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9573), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9573) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9575), new DateTime(2026, 3, 2, 4, 48, 13, 589, DateTimeKind.Utc).AddTicks(9575) });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CostPrice", table: "Products");
            migrationBuilder.DropColumn(name: "LowStockThreshold", table: "Products");
            migrationBuilder.DropColumn(name: "Sku", table: "Products");
            migrationBuilder.DropColumn(name: "StockQuantity", table: "Products");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7390), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7390) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7392), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7393) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7394), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7394) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7396), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7396) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7397), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7397) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7478), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7479) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7481), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7481) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7483), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7483) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7485), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7485) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7486), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7487) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7488), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7489) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7490), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7490) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7492), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7493) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7495), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7495) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7496), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7497) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7517), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7517) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7520), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7520) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7522), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7522) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7524), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7524) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7526), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7526) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7528), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7529) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7530), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7530) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7532), new DateTime(2026, 2, 26, 13, 7, 41, 244, DateTimeKind.Utc).AddTicks(7532) });
        }
    }
}
