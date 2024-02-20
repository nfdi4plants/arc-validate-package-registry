using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageRegistryService.Migrations
{
    /// <inheritdoc />
    public partial class AddContentHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hashes",
                columns: table => new
                {
                    PackageName = table.Column<string>(type: "text", nullable: false),
                    PackageMajorVersion = table.Column<int>(type: "integer", nullable: false),
                    PackageMinorVersion = table.Column<int>(type: "integer", nullable: false),
                    PackagePatchVersion = table.Column<int>(type: "integer", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hashes", x => new { x.PackageName, x.PackageMajorVersion, x.PackageMinorVersion, x.PackagePatchVersion });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hashes");
        }
    }
}
