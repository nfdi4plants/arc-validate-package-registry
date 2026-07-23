using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageRegistryService.Migrations
{
    /// <inheritdoc />
    public partial class AddCLIArguments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CLIArguments",
                table: "ValidationPackages",
                type: "jsonb",
                nullable: true);

            // give pre-existing rows an empty array rather than NULL
            migrationBuilder.Sql(@"UPDATE ""ValidationPackages"" SET ""CLIArguments"" = '[]' WHERE ""CLIArguments"" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CLIArguments",
                table: "ValidationPackages");
        }
    }
}
