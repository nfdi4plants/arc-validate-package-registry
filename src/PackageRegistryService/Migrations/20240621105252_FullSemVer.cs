using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageRegistryService.Migrations
{
    /// <inheritdoc />
    public partial class FullSemVer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ValidationPackages",
                table: "ValidationPackages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Hashes",
                table: "Hashes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Downloads",
                table: "Downloads");

            migrationBuilder.AddColumn<string>(
                name: "PreReleaseVersionSuffix",
                table: "ValidationPackages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuildMetadataVersionSuffix",
                table: "ValidationPackages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackagePreReleaseVersionSuffix",
                table: "Hashes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackageBuildMetadataVersionSuffix",
                table: "Hashes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackagePreReleaseVersionSuffix",
                table: "Downloads",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackageBuildMetadataVersionSuffix",
                table: "Downloads",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ValidationPackages",
                table: "ValidationPackages",
                columns: new[] { "Name", "MajorVersion", "MinorVersion", "PatchVersion", "PreReleaseVersionSuffix", "BuildMetadataVersionSuffix" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Hashes",
                table: "Hashes",
                columns: new[] { "PackageName", "PackageMajorVersion", "PackageMinorVersion", "PackagePatchVersion", "PackagePreReleaseVersionSuffix", "PackageBuildMetadataVersionSuffix" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Downloads",
                table: "Downloads",
                columns: new[] { "PackageName", "PackageMajorVersion", "PackageMinorVersion", "PackagePatchVersion", "PackagePreReleaseVersionSuffix", "PackageBuildMetadataVersionSuffix" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ValidationPackages",
                table: "ValidationPackages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Hashes",
                table: "Hashes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Downloads",
                table: "Downloads");

            migrationBuilder.DropColumn(
                name: "PreReleaseVersionSuffix",
                table: "ValidationPackages");

            migrationBuilder.DropColumn(
                name: "BuildMetadataVersionSuffix",
                table: "ValidationPackages");

            migrationBuilder.DropColumn(
                name: "PackagePreReleaseVersionSuffix",
                table: "Hashes");

            migrationBuilder.DropColumn(
                name: "PackageBuildMetadataVersionSuffix",
                table: "Hashes");

            migrationBuilder.DropColumn(
                name: "PackagePreReleaseVersionSuffix",
                table: "Downloads");

            migrationBuilder.DropColumn(
                name: "PackageBuildMetadataVersionSuffix",
                table: "Downloads");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ValidationPackages",
                table: "ValidationPackages",
                columns: new[] { "Name", "MajorVersion", "MinorVersion", "PatchVersion" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Hashes",
                table: "Hashes",
                columns: new[] { "PackageName", "PackageMajorVersion", "PackageMinorVersion", "PackagePatchVersion" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Downloads",
                table: "Downloads",
                columns: new[] { "PackageName", "PackageMajorVersion", "PackageMinorVersion", "PackagePatchVersion" });
        }
    }
}
