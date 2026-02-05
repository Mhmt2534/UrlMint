using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlMint.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForExpireDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "ShortUrls",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "NOW() + INTERVAL '30 days'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_ExpiresAt",
                table: "ShortUrls",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShortUrls_ExpiresAt",
                table: "ShortUrls");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "ShortUrls",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "NOW() + INTERVAL '30 days'");
        }
    }
}
