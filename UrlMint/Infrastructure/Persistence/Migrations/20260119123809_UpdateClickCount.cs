using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlMint.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClickCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ClickCount",
                table: "ShortUrls",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ClickCount",
                table: "ShortUrls",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
