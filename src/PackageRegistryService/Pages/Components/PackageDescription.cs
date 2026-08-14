namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders multiline package descriptions for detail and summary views.
    /// </summary>
    public class PackageDescription
    {
        /// <summary>Renders description lines as block paragraphs.</summary>
        public static string Render(string description)
        {
            return String.Join(
                System.Environment.NewLine,
                description
                    .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Select(l => $@"<p style=""display:block"">{l}</p>")
            );
            
        }
        /// <summary>Renders description lines using compact text markup.</summary>
        public static string RenderSmall(string description)
        {
            return String.Join(
                System.Environment.NewLine,
                description
                    .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Select(l => $@"<small style=""display:block"">{l}</small>")
            );

        }
    }
}
