using PackageRegistryService.API.Handlers;

namespace PackageRegistryService.API.Endpoints;

public static class DiscoveryEndpointsV1
{
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
