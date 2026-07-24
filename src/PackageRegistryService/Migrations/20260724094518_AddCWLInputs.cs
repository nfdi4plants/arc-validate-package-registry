using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageRegistryService.Migrations
{
    /// <inheritdoc />
    public partial class AddCWLInputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Inputs",
                table: "ValidationPackages",
                type: "jsonb",
                nullable: true);

            // Keep the F# model's empty-collection default for packages that were
            // persisted before CWL inputs were introduced.
            migrationBuilder.Sql(
                @"UPDATE ""ValidationPackages"" SET ""Inputs"" = '[]' WHERE ""Inputs"" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inputs",
                table: "ValidationPackages");
        }
    }
}
