namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders the registry website's landing-page content.
    /// </summary>
    public class Index
    {
        /// <summary>Renders the landing-page HTML.</summary>
        /// <returns>The landing-page content.</returns>
        public static string Render()
        {
            return @"<section>
<h1><strong>AVPR:</strong> ARC validation package registry</h1>
<p><a href=""/packages"">Browse all available packages</a></p>
<p>Learn <a href=""/docs/index.md#about-avpr"">about AVPR</a> or read the <a href=""/docs"">documentation</a>.</p>
<p>For <b>programmatic access</b>, see the <a href=""/swagger"">API documentation</a>.</p>
<hr/>
</section>";
        }
    }
}
