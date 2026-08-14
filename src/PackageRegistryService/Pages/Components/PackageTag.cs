namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders package-tag links used by package pages.
    /// </summary>
    public class PackageTag
    {
        /// <summary>Renders a link that filters package browsing by one tag.</summary>
        public static string RenderLink(string tagName) => $@"<a href=""/packages?tag={tagName}"">{tagName}</a>";
        
        /// <summary>Renders a semicolon-separated list of package-tag links.</summary>
        public static string RenderAllLinksInline(string[]? tagNames)
        {             
            if (tagNames == null)
            {
                return "";
            }
            else
            {
                return String.Join("; ", tagNames.Select(t => RenderLink(t)));
            }
        }
    }
}
