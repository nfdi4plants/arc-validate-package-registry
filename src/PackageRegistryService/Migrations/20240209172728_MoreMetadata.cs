using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageRegistryService.Migrations
{
    /// <inheritdoc />
    public partial class MoreMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Authors",
                table: "ValidationPackages",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseNotes",
                table: "ValidationPackages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Tags",
                table: "ValidationPackages",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Authors",
                table: "ValidationPackages");

            migrationBuilder.DropColumn(
                name: "ReleaseNotes",
                table: "ValidationPackages");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "ValidationPackages");
        }
    }
}
