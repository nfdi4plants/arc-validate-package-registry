using PackageRegistryService.API.Handlers;
using PackageRegistryService.Authentication;

namespace PackageRegistryService.API.Endpoints
{
    /// <summary>
    /// Registers package-content verification endpoints for API version 1.
    /// </summary>
    public static class VerificationEndpointsV1
    {
        /// <summary>
        /// Maps the endpoint that verifies a submitted package-content hash.
        /// </summary>
        /// <param name="group">The verification route group.</param>
        /// <returns>The route group with verification endpoints registered.</returns>
        public static RouteGroupBuilder MapVerificationApiV1(this RouteGroupBuilder group)
        {
            group.MapPost("/", VerificationHandlers.Verify)
                .WithOpenApi()
                .WithName("VerifyPackageContent");

            // remove this at it is safer to create hash entries automatically on posted packages
            //group.MapPost("/hashes", VerificationHandlers.CreateContentHash)
            //    .WithOpenApi()
            //    .WithName("CreatePackageContentHash")
            //    .AddEndpointFilter<APIKeyEndpointFilter>(); // creating hashes via post requests requires an API key!

            return group.WithTags("Content Verification");
        }
    }
}
