namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders author labels used by package pages.
    /// </summary>
    public class PackageAuthor
    {
        /// <summary>Renders one author label.</summary>
        public static string RenderLink(string authorName) => $@"<u>{authorName}</u>";

        /// <summary>Renders a semicolon-separated list of author labels.</summary>
        public static string RenderAllLinksInline(string[]? authorNames)
        {
            if (authorNames == null)
            {
                return "";
            }
            else
            {
                return String.Join("; ", authorNames.Select(t => RenderLink(t)));
            }
        }
    }
}
