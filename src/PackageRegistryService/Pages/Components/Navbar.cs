namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders the shared website navigation bar.
    /// </summary>
    public static class Navbar
    {
        /// <summary>Renders one navigation item and marks it when active.</summary>
        public static string RenderNavbarItem(string active, string item, string link)
        {
            if (active == item)
            {
                return $@"<li><strong><a aria-current=""page"" href=""{link}""><u>{item}</u></a></strong></li>";
            }
            else
            {
                return $@"<li><a href=""{link}"">{item}</a></li>";
            }
        }
        /// <summary>
        /// Renders the complete website navigation bar.
        /// </summary>
        /// <param name="active">The label of the active navigation item.</param>
        /// <returns>The navigation HTML.</returns>
        public static string Render(string active)
        {
    // this should eventually point to knowledge base articles
            return $@"<nav>
  <ul>
    <li><strong>AVPR - a service by <a href=""https://nfdi4plants.org/"">DataPLANT</a></strong></li>
  </ul>
  <ul>
    {RenderNavbarItem(active, "Home", "/")}
    {RenderNavbarItem(active, "Browse Packages", "/packages")}
    {RenderNavbarItem(active, "Documentation", "/docs")}
    {RenderNavbarItem(active, "Releases", "/releases")}
    {RenderNavbarItem(active, "About", "/docs/index.md#about-avpr")}
    <li><a href=""https://github.com/nfdi4plants/arc-validate-package-registry/blob/dev/docs/packages/submission.md"">Submit a package</a></li>
  </ul>
</nav>";
        }
    }
}
