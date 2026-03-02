using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class FixSettingsKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(718), new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(718) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(720), new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(720) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(722), new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(722) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(723), new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(724) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(726), new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(726) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "CreatedAt", "Key", "UpdatedAt", "Value" },
                values: new object[] { "General", new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(818), "site_name", new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(818), "701 Performance Power" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "CreatedAt", "Key", "UpdatedAt" },
                values: new object[] { "General", new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(821), "site_tagline", new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(821) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "CreatedAt", "Key", "SortOrder", "UpdatedAt", "Value" },
                values: new object[] { "General", new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(823), "contact_email", 3, new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(823), "info@701performancepower.com" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "CreatedAt", "IsRequired", "Key", "SortOrder", "UpdatedAt", "Value" },
                values: new object[] { "General", new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(825), false, "contact_phone", 4, new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(825), "" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "CreatedAt", "Key", "SortOrder", "UpdatedAt", "Value" },
                values: new object[] { "General", new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(827), "contact_address", 5, new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(827), "" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "CreatedAt", "Description", "DisplayName", "IsRequired", "Key", "SortOrder", "UpdatedAt", "Value" },
                values: new object[] { "General", new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(828), "Main heading on the homepage hero section", "Homepage Hero Title", false, "hero_title", 6, new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(829), "Discover Your Next Adventure" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "CreatedAt", "Description", "DisplayName", "IsRequired", "Key", "SortOrder", "Type", "UpdatedAt", "Value" },
                values: new object[] { "General", new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(830), "Subtitle text below the hero title", "Homepage Hero Subtitle", false, "hero_subtitle", 7, 1, new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(831), "Premium powersports vehicles and gear for every adventure seeker" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "CreatedAt", "Description", "DisplayName", "Key", "SortOrder", "Type", "UpdatedAt", "Value" },
                values: new object[] { "Security", new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(832), "Whether new users can register on the site", "Allow User Registration", "allow_user_registration", 1, 7, new DateTime(2026, 3, 2, 5, 36, 42, 862, DateTimeKind.Utc).AddTicks(832), "true" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4291), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4292) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4293), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4294) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4295), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4295) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4297), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4297) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4298), new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4298) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "CreatedAt", "Key", "UpdatedAt", "Value" },
                values: new object[] { "Company Info", new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4379), "company.name", new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4379), "PowerSports Showcase" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "CreatedAt", "Key", "UpdatedAt" },
                values: new object[] { "Company Info", new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4381), "company.tagline", new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4382) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "CreatedAt", "Key", "SortOrder", "UpdatedAt", "Value" },
                values: new object[] { "Contact Info", new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4384), "contact.email", 1, new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4384), "info@powersportsshowcase.com" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "CreatedAt", "IsRequired", "Key", "SortOrder", "UpdatedAt", "Value" },
                values: new object[] { "Contact Info", new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4385), true, "contact.phone", 2, new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4386), "(555) 123-4567" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "CreatedAt", "Key", "SortOrder", "UpdatedAt", "Value" },
                values: new object[] { "Contact Info", new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4387), "contact.address", 3, new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4388), "123 Adventure Lane, Outdoor City, OC 12345" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "CreatedAt", "Description", "DisplayName", "IsRequired", "Key", "SortOrder", "UpdatedAt", "Value" },
                values: new object[] { "Footer", new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4389), "Copyright notice displayed in the footer", "Copyright Text", true, "footer.copyright", 1, new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4390), "© 2026 PowerSports Showcase. All rights reserved." });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "CreatedAt", "Description", "DisplayName", "IsRequired", "Key", "SortOrder", "Type", "UpdatedAt", "Value" },
                values: new object[] { "Homepage", new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4391), "Main heading on the homepage hero section", "Homepage Hero Title", true, "homepage.hero.title", 1, 0, new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4391), "Discover Your Next Adventure" });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "CreatedAt", "Description", "DisplayName", "Key", "SortOrder", "Type", "UpdatedAt", "Value" },
                values: new object[] { "Homepage", new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4393), "Subtitle text below the hero title", "Homepage Hero Subtitle", "homepage.hero.subtitle", 2, 1, new DateTime(2026, 3, 2, 5, 16, 4, 991, DateTimeKind.Utc).AddTicks(4393), "Premium powersports vehicles and gear for every adventure seeker" });
        }
    }
}
