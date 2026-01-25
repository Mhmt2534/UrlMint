using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlMint.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShortCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortCode",
                table: "ShortUrls",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortCode",
                table: "ShortUrls");
        }
    }
}
