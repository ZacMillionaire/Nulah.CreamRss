using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nulah.RSS.Data.Migrations
{
    /// <inheritdoc />
    public partial class Favicon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FaviconBlob",
                table: "Feeds",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaviconBlob",
                table: "Feeds");
        }
    }
}
