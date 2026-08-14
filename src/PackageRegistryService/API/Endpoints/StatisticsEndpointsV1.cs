using PackageRegistryService.API.Handlers;
using PackageRegistryService.Authentication;

namespace PackageRegistryService.API.Endpoints
{
    /// <summary>
    /// Registers package download-statistics endpoints for API version 1.
    /// </summary>
    public static class StatisticsEndpointsV1
    {
        /// <summary>
        /// Maps aggregate and package-specific download-statistics routes.
        /// </summary>
        /// <param name="group">The statistics route group.</param>
        /// <returns>The route group with statistics endpoints registered.</returns>
        public static RouteGroupBuilder MapStatisticsApiV1(this RouteGroupBuilder group)
        {

            // packages endpoints
            group.MapGet("/downloads", DownloadsHandlers.GetAllDownloads)
                .WithOpenApi()
                .WithName("GetAllDownloads");

            group.MapGet("/downloads/{name}", DownloadsHandlers.GetAllDownloadsByName)
                .WithOpenApi()
                .WithName("GetAllDownloadsByName");

            group.MapGet("/downloads/{name}/{version}", DownloadsHandlers.GetDownloadsByNameAndVersion)
                .WithOpenApi()
                .WithName("GetDownloadsByNameAndVersion");

            return group.WithTags("Statistics"); ;
        }
    }
}
