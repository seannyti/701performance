using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSessionToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add with empty-string default, back-fill existing rows, then remove default.
            migrationBuilder.AddColumn<string>(
                name: "SessionToken",
                table: "ChatSessions",
                type: "varchar(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE ChatSessions SET SessionToken = UUID() WHERE SessionToken = ''");

            migrationBuilder.Sql("ALTER TABLE ChatSessions ALTER COLUMN SessionToken DROP DEFAULT");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_SessionToken",
                table: "ChatSessions",
                column: "SessionToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_SessionToken",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "SessionToken",
                table: "ChatSessions");
        }
    }
}
