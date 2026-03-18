using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PowersportsApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedHeroVideoSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_SessionToken",
                table: "ChatSessions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "VerificationEmailLastSentAt",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<bool>(
                name: "SubscribeNewsletter",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LockoutEnd",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "LastLoginIp",
                table: "Users",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEmailVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EmailVerificationTokenExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmailVerificationToken",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SiteSettings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SiteSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SiteSettings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "SiteSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "RefreshTokens",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsFeatured",
                table: "Products",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<bool>(
                name: "IsMain",
                table: "ProductImages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductImages",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductImages",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "TrackingNumber",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingZipCode",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingState",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCountry",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCity",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCarrier",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ShippedDate",
                table: "Orders",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentReceivedDate",
                table: "Orders",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentNotes",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeliveredDate",
                table: "Orders",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerPhone",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerNotes",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "AdminNotes",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "ProductSku",
                table: "OrderItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "OrderItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "MediaSections",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MediaSections",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                table: "MediaSections",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "MediaSections",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MediaSections",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MediaSections",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "MediaSections",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UploadedAt",
                table: "MediaFiles",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "MediaFiles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ThumbnailPath",
                table: "MediaFiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "MediaFiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StoredFileName",
                table: "MediaFiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "MimeType",
                table: "MediaFiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "MediaFiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "MediaFiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "MediaFiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "MediaFiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "MediaFiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContactSubmissions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "ContactSubmissions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReadAt",
                table: "ContactSubmissions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ContactSubmissions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ContactSubmissions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ContactSubmissions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ContactSubmissions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "AdminNotes",
                table: "ContactSubmissions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ContactSubmissions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ChatSessions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "SessionToken",
                table: "ChatSessions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "GuestName",
                table: "ChatSessions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GuestEmail",
                table: "ChatSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClosedAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AgentConnectionId",
                table: "ChatSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ChatSessions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "ChatMessages",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "SenderName",
                table: "ChatMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)",
                oldDefaultValue: (byte)0);

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ChatMessages",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CategoryImages",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CategoryImages",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Categories",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Categories",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Categories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "Appointments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceType",
                table: "Appointments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Appointments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Appointments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerPhone",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Appointments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "Appointments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Appointments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsActive", "IsPublic", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7776), true, true, true, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7776) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsActive", "IsPublic", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7779), true, true, false, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7779) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "IsActive", "IsPublic", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7781), true, true, true, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7781) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "IsActive", "IsPublic", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7807), true, true, false, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7808) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "IsActive", "IsPublic", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7809), true, true, false, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7810) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "IsActive", "IsPublic", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7811), true, true, false, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7812) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "IsActive", "IsPublic", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7813), true, true, false, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7814) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "IsActive", "IsPublic", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7821), true, false, false, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7822) });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "DisplayName", "IsActive", "IsPublic", "IsRequired", "Key", "LastModifiedBy", "SortOrder", "Type", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 9, "Content", new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7815), "Show a video instead of a static image in the hero section", "Enable Hero Video Background", true, true, false, "hero_video_enabled", null, 8, 7, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7816), "false" },
                    { 10, "Content", new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7817), "YouTube URL or uploaded MP4 path for the hero background video", "Hero Video URL", true, true, false, "hero_video_url", null, 9, 5, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7818), "" },
                    { 11, "Content", new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7819), "Number of seconds to skip at the start of the video", "Hero Video Start Time", true, true, false, "hero_video_start", null, 10, 8, new DateTime(2026, 3, 18, 12, 7, 32, 91, DateTimeKind.Utc).AddTicks(7820), "0" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "SiteSettings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "VerificationEmailLastSentAt",
                table: "Users",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<byte>(
                name: "SubscribeNewsletter",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LockoutEnd",
                table: "Users",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "LastLoginIp",
                table: "Users",
                type: "varchar(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "IsEmailVerified",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<byte>(
                name: "IsActive",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EmailVerificationTokenExpiry",
                table: "Users",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmailVerificationToken",
                table: "Users",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "SiteSettings",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SiteSettings",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SiteSettings",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<byte>(
                name: "IsRequired",
                table: "SiteSettings",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<byte>(
                name: "IsActive",
                table: "SiteSettings",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "SiteSettings",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SiteSettings",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SiteSettings",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "SiteSettings",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "RefreshTokens",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshTokens",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "RefreshTokens",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "Products",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<byte>(
                name: "IsFeatured",
                table: "Products",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<byte>(
                name: "IsActive",
                table: "Products",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Products",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<byte>(
                name: "IsMain",
                table: "ProductImages",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductImages",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ProductImages",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "TrackingNumber",
                table: "Orders",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingZipCode",
                table: "Orders",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingState",
                table: "Orders",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCountry",
                table: "Orders",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCity",
                table: "Orders",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCarrier",
                table: "Orders",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ShippedDate",
                table: "Orders",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentReceivedDate",
                table: "Orders",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentNotes",
                table: "Orders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeliveredDate",
                table: "Orders",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerPhone",
                table: "Orders",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerNotes",
                table: "Orders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "Orders",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "AdminNotes",
                table: "Orders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "ProductSku",
                table: "OrderItems",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "OrderItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "MediaSections",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MediaSections",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<byte>(
                name: "IsSystem",
                table: "MediaSections",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<byte>(
                name: "IsActive",
                table: "MediaSections",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MediaSections",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MediaSections",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "MediaSections",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UploadedAt",
                table: "MediaFiles",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "MediaFiles",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ThumbnailPath",
                table: "MediaFiles",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "MediaFiles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StoredFileName",
                table: "MediaFiles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "MimeType",
                table: "MediaFiles",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "MediaFiles",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "MediaFiles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "MediaFiles",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "MediaFiles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "MediaFiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContactSubmissions",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "ContactSubmissions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReadAt",
                table: "ContactSubmissions",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ContactSubmissions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ContactSubmissions",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ContactSubmissions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ContactSubmissions",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "AdminNotes",
                table: "ContactSubmissions",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ContactSubmissions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ChatSessions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "SessionToken",
                table: "ChatSessions",
                type: "varchar(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "GuestName",
                table: "ChatSessions",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GuestEmail",
                table: "ChatSessions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ChatSessions",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClosedAt",
                table: "ChatSessions",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AgentConnectionId",
                table: "ChatSessions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ChatSessions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "ChatMessages",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "SenderName",
                table: "ChatMessages",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<byte>(
                name: "IsRead",
                table: "ChatMessages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "ChatMessages",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ChatMessages",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CategoryImages",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CategoryImages",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Categories",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<byte>(
                name: "IsActive",
                table: "Categories",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Categories",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Categories",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Categories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Appointments",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "Appointments",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceType",
                table: "Appointments",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Appointments",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Appointments",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerPhone",
                table: "Appointments",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Appointments",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "Appointments",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Appointments",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Appointments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsActive", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2448), (byte)1, (byte)1, new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2448) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsActive", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2451), (byte)1, (byte)0, new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2451) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "IsActive", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2453), (byte)1, (byte)1, new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2453) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "IsActive", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2455), (byte)1, (byte)0, new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2455) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "IsActive", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2457), (byte)1, (byte)0, new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2457) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "IsActive", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2459), (byte)1, (byte)0, new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2459) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "IsActive", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2461), (byte)1, (byte)0, new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2461) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "IsActive", "IsRequired", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2463), (byte)1, (byte)0, new DateTime(2026, 3, 16, 0, 47, 56, 613, DateTimeKind.Utc).AddTicks(2463) });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_SessionToken",
                table: "ChatSessions",
                column: "SessionToken",
                unique: true);
        }
    }
}
