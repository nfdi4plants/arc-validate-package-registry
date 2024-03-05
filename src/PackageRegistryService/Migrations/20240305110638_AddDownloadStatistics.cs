using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageRegistryService.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Downloads",
                columns: table => new
                {
                    PackageName = table.Column<string>(type: "text", nullable: false),
                    PackageMajorVersion = table.Column<int>(type: "integer", nullable: false),
                    PackageMinorVersion = table.Column<int>(type: "integer", nullable: false),
                    PackagePatchVersion = table.Column<int>(type: "integer", nullable: false),
                    Downloads = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Downloads", x => new { x.PackageName, x.PackageMajorVersion, x.PackageMinorVersion, x.PackagePatchVersion });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Downloads");
        }
    }
}
