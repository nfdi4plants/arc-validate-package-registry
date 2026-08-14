using PackageRegistryService.API.Handlers;

namespace PackageRegistryService.API.Endpoints;

/// <summary>
/// Registers lightweight validation-package discovery endpoints for API version 1.
/// </summary>
public static class DiscoveryEndpointsV1
{
    /// <summary>
    /// Maps package identity, version-list, and metadata discovery routes.
    /// </summary>
    /// <param name="group">The API version 1 route group.</param>
    /// <returns>The route group with discovery endpoints registered.</returns>
    public static RouteGroupBuilder MapDiscoveryApiV1(this RouteGroupBuilder group)
    {
        group.MapGet("/package-index", PackageDiscoveryHandlers.GetPackageIndex)
            .WithOpenApi()
            .WithName("GetPackageIndex");

        group.MapGet("/packages/{name}/versions", PackageDiscoveryHandlers.GetPackageVersions)
            .WithOpenApi()
            .WithName("GetPackageVersions");

        group.MapGet(
                "/packages/{name}/{version}/metadata",
                PackageDiscoveryHandlers.GetPackageMetadata)
            .WithOpenApi()
            .WithName("GetPackageMetadata");

        return group.WithTags("Validation Package Discovery");
    }
}
