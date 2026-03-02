using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLastLoginIp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastLoginIp",
                table: "Users",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3595), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3595) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3597), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3597) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3598), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3599) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3600), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3600) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3601), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3602) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3689), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3690) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3692), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3692) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3694), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3695) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3697), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3699) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3701), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3701) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3704), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3705) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3707), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3707) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3710), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3710) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3713), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3713) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3717), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3718) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3743), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3743) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3745), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3746) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3748), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3748) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3750), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3750) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3753), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3753) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3756), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3757) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3758), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3759) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3760), new DateTime(2026, 2, 26, 12, 8, 40, 56, DateTimeKind.Utc).AddTicks(3760) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginIp",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8431), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8431) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8433), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8433) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8436), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8436) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8439), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8440) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8441), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8442) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8525), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8526) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8529), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8529) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8531), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8531) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8532), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8533) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8534), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8535) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8537), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8537) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8539), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8539) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8540), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8541) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8542), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8542) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8544), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8544) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8565), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8565) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8570), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8571) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8572), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8573) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8575), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8576) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8578), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8578) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8581), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8581) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8584), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8584) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8586), new DateTime(2026, 2, 25, 9, 8, 19, 16, DateTimeKind.Utc).AddTicks(8586) });
        }
    }
}
