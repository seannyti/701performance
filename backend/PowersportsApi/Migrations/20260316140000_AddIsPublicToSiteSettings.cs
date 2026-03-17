using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublicToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "SiteSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            // Mark all content/presentation settings as public.
            // Anything in Email, Advanced, Security, or System categories stays private (IsPublic = false).
            migrationBuilder.Sql(@"
                UPDATE SiteSettings
                SET IsPublic = 1
                WHERE Category NOT IN ('Email', 'Advanced', 'Security', 'System');
            ");

            // enable_maintenance_mode lives in 'Advanced' but the frontend needs it to
            // redirect visitors to the maintenance page — make it public explicitly.
            migrationBuilder.Sql(@"
                UPDATE SiteSettings
                SET IsPublic = 1
                WHERE `Key` = 'enable_maintenance_mode';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "SiteSettings");
        }
    }
}
