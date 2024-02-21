using PackageRegistryService.API.Handlers;
using PackageRegistryService.Authentication;

namespace PackageRegistryService.API.Endpoints
{
    public static class VerificationEndpointsV1
    {
        public static RouteGroupBuilder MapVerificationApiV1(this RouteGroupBuilder group)
        {
            group.MapPost("/{name}/{version}", VerificationHandlers.Verify)
                .WithOpenApi()
                .WithName("Verify");

            return group.WithTags("Content Verification");
        }
    }
}
