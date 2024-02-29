using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageRegistryService.Migrations
{
    /// <inheritdoc />
    public partial class AddSummaryAndOntologyTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
            migrationBuilder.Sql(@"
                CREATE FUNCTION transform_tags(tags text[]) RETURNS jsonb AS $$
                BEGIN
                    RETURN jsonb_agg(jsonb_build_object('Name', tag)) FROM unnest(tags) AS tag;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""ValidationPackages""
                ADD COLUMN ""TMP"" jsonb;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""ValidationPackages""
                SET ""TMP"" = transform_tags(""Tags"");
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""ValidationPackages""
                DROP COLUMN ""Tags"";
                ALTER TABLE ""ValidationPackages""
                RENAME COLUMN ""TMP"" TO ""Tags"";
            ");
                
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "ValidationPackages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS transform_tags(text[]);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""ValidationPackages""
                ADD COLUMN ""TMP"" text[];
            ");

            migrationBuilder.Sql(@"
                UPDATE ""ValidationPackages""
                SET ""TMP"" = (
                    SELECT array_agg(tag->>'Name') FROM jsonb_array_elements(""Tags"") AS tag
                );
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""ValidationPackages""
                DROP COLUMN ""Tags"";
                ALTER TABLE ""ValidationPackages""
                RENAME COLUMN ""TMP"" TO ""Tags"";
            ");

            migrationBuilder.AlterColumn<string[]>(
                name: "Tags",
                table: "ValidationPackages",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
