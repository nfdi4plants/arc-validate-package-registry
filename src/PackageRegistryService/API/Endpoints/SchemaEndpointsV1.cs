using PackageRegistryService.API.Handlers;

namespace PackageRegistryService.API.Endpoints;

/// <summary>
/// Registers versioned JSON Schema resources published by the service.
/// </summary>
public static class SchemaEndpointsV1
{
    /// <summary>
    /// Maps the public schema-document routes.
    /// </summary>
    /// <param name="app">The web application being configured.</param>
    /// <returns>The application with schema endpoints registered.</returns>
    public static WebApplication MapSchemaEndpointsV1(this WebApplication app)
    {
        app.MapGet(
            "/schemas/v1/validation-package-frontmatter.schema.json",
            SchemaHandlers.GetValidationPackageFrontmatter);

        app.MapGet(
            "/schemas/v1/validation-packages.schema.json",
            SchemaHandlers.GetValidationPackagesConfig);

        return app;
    }
}
