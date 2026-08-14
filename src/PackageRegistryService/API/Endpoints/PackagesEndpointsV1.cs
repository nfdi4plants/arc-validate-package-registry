using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Authentication;
using Microsoft.AspNetCore.Mvc;
using PackageRegistryService.API.Handlers;
 
namespace PackageRegistryService.API.Endpoints
{ 
    /// <summary>
    /// Registers validation-package content endpoints for API version 1.
    /// </summary>
    public static class PackagesEndpointsV1
    {
        /// <summary>
        /// Maps package retrieval and authenticated package-creation routes.
        /// </summary>
        /// <param name="group">The package route group.</param>
        /// <returns>The route group with package endpoints registered.</returns>
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
