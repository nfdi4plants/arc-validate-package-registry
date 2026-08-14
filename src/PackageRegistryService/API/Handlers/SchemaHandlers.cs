namespace PackageRegistryService.API.Handlers;

public static class SchemaHandlers
{
    private const string SchemaMediaType = "application/schema+json";

    public static IResult GetValidationPackageFrontmatter() =>
        ReadSchema("validation-package-frontmatter.schema.json");

    public static IResult GetValidationPackagesConfig() =>
        ReadSchema("validation-packages.schema.json");

    private static IResult ReadSchema(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "schemas", fileName);
        return TypedResults.Bytes(File.ReadAllBytes(path), SchemaMediaType);
    }
}
