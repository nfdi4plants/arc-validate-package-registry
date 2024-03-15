using System;

namespace PackageRegistryService.Pages.Components
{
    public class Footer
    {
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
        {RenderFooterItem(active, "About", "/about")}
        {RenderFooterItem(active, "Submit a package", "https://github.com/nfdi4plants/arc-validate-package-registry?tab=readme-ov-file#validation-package-staging-area")}
      </ul>
    </div>
  </div>
</footer>";
        }
    }
}
