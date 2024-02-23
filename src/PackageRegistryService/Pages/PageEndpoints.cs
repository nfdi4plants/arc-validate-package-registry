namespace PackageRegistryService.Pages
{
    public static class PageEndpoints
    {
        public static RouteGroupBuilder MapPageEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("", Index.Render);

            group.MapGet("packages", Packages.Render);

            group.MapGet("package/{packageName}", Package.Render);

            return group;
        }
    }
}
