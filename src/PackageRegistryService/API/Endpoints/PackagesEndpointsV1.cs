using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Authentication;
using Microsoft.AspNetCore.Mvc;
using PackageRegistryService.API.Handlers;
 
namespace PackageRegistryService.API.Endpoints
{
    public static class PackagesEndpointsV1
    {
        public static RouteGroupBuilder MapPackagesApiV1(this RouteGroupBuilder group)
        {

            // packages endpoints
            group.MapGet("/", PackageHandlers.GetAllPackages)
                .WithOpenApi()
                .WithName("GetAllPackages");

            group.MapGet("/{name}", PackageHandlers.GetLatestPackageByName)
                .WithOpenApi()
                .WithName("GetLatestPackageByName");

            group.MapGet("/{name}/{version}", PackageHandlers.GetPackageByNameAndVersion)
                .WithOpenApi()
                .WithName("GetPackageByNameAndVersion");

            group.MapPost("/", PackageHandlers.CreatePackage)
                .WithOpenApi()
                .WithName("CreatePackage")
                .AddEndpointFilter<APIKeyEndpointFilter>(); // creating packages via post requests requires an API key

            return group.WithTags("Validation Packages");
        }
    }
}
