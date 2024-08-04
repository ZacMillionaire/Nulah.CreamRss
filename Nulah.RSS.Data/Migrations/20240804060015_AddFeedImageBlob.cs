using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nulah.RSS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedImageBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBlob",
                table: "Feeds",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageBlob",
                table: "Feeds");
        }
    }
}
