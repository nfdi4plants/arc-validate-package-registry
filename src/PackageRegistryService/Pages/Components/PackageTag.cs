namespace PackageRegistryService.Pages.Components
{
    public class PackageTag
    {
        public static string RenderLink(string tagName) => $@"<a href=""/packages?tag={tagName}"">{tagName}</a>";
        
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
