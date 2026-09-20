using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCodeToHashAndUseDateTimeOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "EmailConfirmations");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "EmailConfirmations",
                type: "datetimeoffset",
                nullable: false,
                comment: "the expiration date is required",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "EmailConfirmations",
                type: "datetimeoffset",
                nullable: false,
                comment: "the creation date is required",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CodeHash",
                table: "EmailConfirmations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeHash",
                table: "EmailConfirmations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "EmailConfirmations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldComment: "the expiration date is required");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "EmailConfirmations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldComment: "the creation date is required");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "EmailConfirmations",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");
        }
    }
}
