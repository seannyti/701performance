using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(86), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(86) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(88), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(88) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(90), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(90) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(91), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(91) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(93), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(93) });

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
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(178), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(179) });

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
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(186), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(186) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(188), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(188) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(190), new DateTime(2026, 3, 14, 18, 22, 42, 123, DateTimeKind.Utc).AddTicks(190) });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CreatedByUserId",
                table: "Appointments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_UserId",
                table: "Appointments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

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
    }
}
