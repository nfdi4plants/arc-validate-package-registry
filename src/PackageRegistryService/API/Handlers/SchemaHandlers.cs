namespace PackageRegistryService.API.Handlers;

/// <summary>
/// Serves the versioned JSON Schema resources bundled with the application.
/// </summary>
public static class SchemaHandlers
{
    private const string SchemaMediaType = "application/schema+json";

    /// <summary>
    /// Returns the schema for validation-package frontmatter.
    /// </summary>
    /// <returns>The schema document with the JSON Schema media type.</returns>
    public static IResult GetValidationPackageFrontmatter() =>
        ReadSchema("validation-package-frontmatter.schema.json");

    /// <summary>
    /// Returns the schema for validation-package execution configuration.
    /// </summary>
    /// <returns>The schema document with the JSON Schema media type.</returns>
    public static IResult GetValidationPackagesConfig() =>
        ReadSchema("validation-packages.schema.json");

    /// <summary>
    /// Reads one bundled schema and creates its HTTP result.
    /// </summary>
    /// <param name="fileName">The schema filename within the runtime schema directory.</param>
    /// <returns>The schema bytes with the JSON Schema media type.</returns>
    private static IResult ReadSchema(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "schemas", fileName);
        return TypedResults.Bytes(File.ReadAllBytes(path), SchemaMediaType);
    }
}
