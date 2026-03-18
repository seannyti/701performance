using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationEmailLastSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationEmailLastSentAt",
                table: "Users",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationEmailLastSentAt",
                table: "Users");
        }
    }
}
