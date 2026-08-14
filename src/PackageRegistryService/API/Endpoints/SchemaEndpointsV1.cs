using PackageRegistryService.API.Handlers;

namespace PackageRegistryService.API.Endpoints;

public static class SchemaEndpointsV1
{
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
