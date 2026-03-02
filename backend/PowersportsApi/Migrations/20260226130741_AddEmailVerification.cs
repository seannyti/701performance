using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsEmailVerified", table: "Users");
            migrationBuilder.DropColumn(name: "EmailVerificationToken", table: "Users");
            migrationBuilder.DropColumn(name: "EmailVerificationTokenExpiry", table: "Users");

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
    }
}
