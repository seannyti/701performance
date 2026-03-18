using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedHeroVideoSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "Key", "DisplayName", "Value", "Description", "Type", "Category", "SortOrder", "IsRequired", "IsActive", "IsPublic", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { 9,  "hero_video_enabled", "Enable Hero Video Background", "false", "Show a video instead of a static image in the hero section",              7, "Content", 8,  false, true, true, new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, "hero_video_url",     "Hero Video URL",               "",      "YouTube URL or uploaded MP4 path for the hero background video",           5, "Content", 9,  false, true, true, new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, "hero_video_start",   "Hero Video Start Time",        "0",     "Number of seconds to skip at the start of the video",                     8, "Content", 10, false, true, true, new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValues: new object[] { 9, 10, 11 });
        }
    }
}
