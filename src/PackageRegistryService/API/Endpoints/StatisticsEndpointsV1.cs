using PackageRegistryService.API.Handlers;
using PackageRegistryService.Authentication;

namespace PackageRegistryService.API.Endpoints
{
    public static class StatisticsEndpointsV1
    {
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
