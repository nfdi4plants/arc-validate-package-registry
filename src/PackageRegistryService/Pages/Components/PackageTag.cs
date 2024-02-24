namespace PackageRegistryService.Pages.Components
{
    public class PackageTag
    {
        public static string Render(string tagName) => $@"<a href=""/packages?tag={tagName}"">{tagName}</a>";
        
        public static string RenderAllInline(string[]? tags)
        {             
            if (tags == null)
            {
                return "";
            }
            else
            {
                return String.Join("; ", tags.Select(t => Render(t)));
            }
        }
    }
}
