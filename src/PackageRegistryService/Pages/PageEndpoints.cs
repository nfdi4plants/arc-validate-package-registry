using PackageRegistryService.Pages.Handlers;

namespace PackageRegistryService.Pages
{
    /// <summary>
    /// Registers routes for the server-rendered registry website.
    /// </summary>
    public static class PageEndpoints
    {
        /// <summary>
        /// Maps landing, documentation, release, browse, and package-detail pages.
        /// </summary>
        /// <param name="group">The website route group.</param>
        /// <returns>The route group with page endpoints registered.</returns>
        public static RouteGroupBuilder MapPageEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("", IndexHandlers.Render);

            group.MapGet("about", AboutHandlers.Render);

            group.MapGet("releases", ReleaseHandlers.Render);

            group.MapGet("docs", DocumentationHandlers.RenderIndex);

            group.MapGet("docs/{**document}", DocumentationHandlers.Render);

            group.MapGet("packages", PackagesHandlers.Render);

            group.MapGet("package/{packageName}", PackageHandlers.RenderLatest);

            group.MapGet("package/{packageName}/{version}", PackageHandlers.Render);

            return group;
        }
    }
}
