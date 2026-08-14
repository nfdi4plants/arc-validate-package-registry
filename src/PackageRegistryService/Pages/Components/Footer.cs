using System;

namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders the shared website footer and its navigation links.
    /// </summary>
    public class Footer
    {
        /// <summary>Renders one footer navigation item and marks it when active.</summary>
        static string RenderFooterItem(string active, string item, string link)
        {
            if (active == item)
            {
                return $@"<li><strong><small><a class=""secondary"" aria-current=""page"" href=""{link}"">{item}</a></small></strong></li>";
            }
            else
            {
                return $@"<li><small><a class=""secondary"" href=""{link}"">{item}</a></small></li>";
            }
        }

        /// <summary>
        /// Renders the complete website footer.
        /// </summary>
        /// <param name="active">The label of the active navigation item.</param>
        /// <returns>The footer HTML.</returns>
        public static string Render(string active) 
        {
            return $@"<footer style=""margin-top: 200px"" class=""container"">
  <hr/>
  <div class=""grid"">
    <div>
      <small><strong>AVPR - a service by <a class=""secondary"" href=""https://nfdi4plants.org/"">DataPLANT</a></strong></small>
  </div>
    <div>
      <ul>
        {RenderFooterItem(active, "Home", "/")}
        {RenderFooterItem(active, "Browse Packages", "/packages")}
        {RenderFooterItem(active, "Documentation", "/docs")}
        {RenderFooterItem(active, "Releases", "/releases")}
        {RenderFooterItem(active, "About", "/docs/index.md#about-avpr")}
        {RenderFooterItem(active, "Submit a package", "https://github.com/nfdi4plants/arc-validate-package-registry/blob/dev/docs/packages/submission.md")}
      </ul>
    </div>
  </div>
</footer>";
        }
    }
}
