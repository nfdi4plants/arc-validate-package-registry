namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders the common HTML document shell used by website pages.
    /// </summary>
    public static class Layout
    {
        /// <summary>
        /// Wraps page content with shared metadata, assets, navigation, and footer markup.
        /// </summary>
        /// <param name="activeNavbarItem">The label of the active navigation item.</param>
        /// <param name="title">The HTML document title.</param>
        /// <param name="content">The page-specific body content.</param>
        /// <param name="additionalHeadContent">Optional page-specific markup for the document head.</param>
        /// <returns>A complete HTML document.</returns>
        public static string Render(
            string activeNavbarItem,
            string title,
            string content,
            string additionalHeadContent = ""
        )
        {
            return $@"<!DOCTYPE html>
<html>
  <head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <meta name=""color-scheme"" content=""light dark"" />
    <link rel=""stylesheet"" href=""/css/pico.cyan.min.css"" />
    <link rel=""stylesheet"" href=""/css/highlightjs.atom-one-dark.min.css"" />
    {additionalHeadContent}
    <script src=""/js/highlight.min.js""></script>
    <script>hljs.highlightAll();</script>
    <title>{title}</title>
  </head>
  <body>
    <header class=""container"">
      <section>
        {Navbar.Render(active: activeNavbarItem)}
      </section>
    </header>
    <main class=""container"">
      <section>
        {content}
      </section>
    </main>
      <section>
      {Footer.Render(active: activeNavbarItem)}
      </section>
  </body>
</html>";
        }
    }
}
