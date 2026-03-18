using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToContactSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContactSubmissions",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ContactSubmissions");
        }
    }
}
