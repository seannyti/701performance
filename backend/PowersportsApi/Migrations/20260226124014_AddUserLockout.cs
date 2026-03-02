using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLockout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(3924), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(3924) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(3926), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(3926) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(3927), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(3928) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(3929), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(3929) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(3931), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(3931) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4008), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4009) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4011), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4011) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4039), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4039) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4042), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4042) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4044), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4044) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4046), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4046) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4048), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4048) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4050), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4050) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4052), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4052) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4054), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4054) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4074), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4075) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4077), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4078) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4080), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4080) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4082), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4082) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4084), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4085) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4086), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4087) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4088), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4089) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4090), new DateTime(2026, 2, 26, 12, 40, 14, 30, DateTimeKind.Utc).AddTicks(4090) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "Users");

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
    }
}
