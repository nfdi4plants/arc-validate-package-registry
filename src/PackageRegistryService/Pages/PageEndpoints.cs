using PackageRegistryService.Pages.Handlers;

namespace PackageRegistryService.Pages
{
    public static class PageEndpoints
    {
        public static RouteGroupBuilder MapPageEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("", IndexHandlers.Render);

            group.MapGet("packages", PackagesHandlers.Render);

            group.MapGet("package/{packageName}", PackageHandlers.RenderLatest);

            group.MapGet("package/{packageName}/{version}", PackageHandlers.Render);

            return group;
        }
    }
}
