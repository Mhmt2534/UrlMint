using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UrlMint.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateDocker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClickLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShortCode = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Referer = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    ClickedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClickLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShortUrls",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LongUrl = table.Column<string>(type: "text", nullable: false),
                    ShortCode = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ClickCount = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW() + INTERVAL '30 days'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortUrls", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_ExpiresAt",
                table: "ShortUrls",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_ShortCode",
                table: "ShortUrls",
                column: "ShortCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClickLogs");

            migrationBuilder.DropTable(
                name: "ShortUrls");
        }
    }
}
