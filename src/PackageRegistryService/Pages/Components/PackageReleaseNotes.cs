namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders multiline package release notes for detail and summary views.
    /// </summary>
    public class PackageReleaseNotes
    {
        /// <summary>Renders release-note lines as block paragraphs.</summary>
        public static string Render(string? releaseNotes)
        {
            if (releaseNotes == null)
            {
                return "<p>No release notes available for this version</p>";
            }
            else
            {
                return String.Join(
                    System.Environment.NewLine,
                    releaseNotes
                        .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                        .Select(l => $@"<p style=""display:block"">{l}</p>")
                );
            }
        }
        /// <summary>Renders release-note lines using compact text markup.</summary>
        public static string RenderSmall(string? releaseNotes)
        {
            if (releaseNotes == null)
            {
                return "<p>No release notes available for this version</p>";
            }
            else
            {
                return String.Join(
                    System.Environment.NewLine,
                    releaseNotes
                        .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                        .Select(l => $@"<small style=""display:block"">{l}</small>")
                );

            }
        }
    }
}
