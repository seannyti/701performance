using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public class SeedHeroVideoSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use INSERT IGNORE so this is safe on both fresh installs and existing
            // databases where these settings may already exist (created via admin panel).
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `SiteSettings` (`Key`, `DisplayName`, `Value`, `Description`, `Type`, `Category`, `SortOrder`, `IsRequired`, `IsActive`, `IsPublic`, `CreatedAt`, `UpdatedAt`)
                VALUES
                    ('hero_video_enabled', 'Enable Hero Video Background', 'false', 'Show a video instead of a static image in the hero section', 7, 'Content', 8, 0, 1, 1, '2026-03-18 00:00:00', '2026-03-18 00:00:00'),
                    ('hero_video_url',     'Hero Video URL',               '',      'YouTube URL or uploaded MP4 path for the hero background video', 5, 'Content', 9, 0, 1, 1, '2026-03-18 00:00:00', '2026-03-18 00:00:00'),
                    ('hero_video_start',   'Hero Video Start Time',        '0',     'Number of seconds to skip at the start of the video', 8, 'Content', 10, 0, 1, 1, '2026-03-18 00:00:00', '2026-03-18 00:00:00');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM `SiteSettings`
                WHERE `Key` IN ('hero_video_enabled', 'hero_video_url', 'hero_video_start');
            ");
        }
    }
}
