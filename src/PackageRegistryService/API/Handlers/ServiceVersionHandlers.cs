using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Models;
using PackageRegistryService.Services;

namespace PackageRegistryService.API.Handlers;

/// <summary>
/// Handles requests for the running service's version and release identity.
/// </summary>
public static class ServiceVersionHandlers
{
    /// <summary>
    /// Returns current service, API, build, and release information without caching.
    /// </summary>
    /// <param name="response">The outgoing response whose cache policy is configured.</param>
    /// <param name="releaseInfo">The current service release information.</param>
    /// <returns>An HTTP result containing the service version document.</returns>
    public static Ok<ServiceVersionDocument> Get(
        HttpResponse response,
        IServiceReleaseInfoProvider releaseInfo
    )
    {
        response.Headers.CacheControl = "no-store";
        return TypedResults.Ok(releaseInfo.Current);
    }
}
