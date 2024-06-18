using PackageRegistryService.API.Handlers;
using PackageRegistryService.Authentication;

namespace PackageRegistryService.API.Endpoints
{
    public static class VerificationEndpointsV1
    {
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
