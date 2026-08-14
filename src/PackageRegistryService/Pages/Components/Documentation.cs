using System.Net;
using System.Text;
using PackageRegistryService.Models;

namespace PackageRegistryService.Pages.Components;

/// <summary>
/// Renders documentation content and its in-page navigation.
/// </summary>
public static class Documentation
{
    /// <summary>
    /// Renders a documentation page with a table of contents.
    /// </summary>
    /// <param name="page">The rendered documentation page.</param>
    /// <returns>The documentation body HTML.</returns>
    public static string Render(DocumentationPage page) =>
        $@"<p><a href=""/docs"">Documentation home</a></p>
<div class=""documentation-layout"">
  <aside class=""documentation-sidebar"">
    <nav aria-label=""On this page"">
      <strong>On this page</strong>
      {RenderTableOfContents(page.Headings)}
    </nav>
  </aside>
  <article class=""documentation-content"">
  {page.Html}
  </article>
</div>";

    /// <summary>
    /// Encodes a documentation page title for placement in the HTML document title.
    /// </summary>
    public static string RenderTitle(DocumentationPage page) =>
        WebUtility.HtmlEncode(page.Title);

    /// <summary>
    /// Renders links to the headings extracted from a documentation page.
    /// </summary>
    private static string RenderTableOfContents(
        IReadOnlyList<MarkdownHeading> headings
    )
    {
        var content = new StringBuilder("<ul>");

        foreach (var heading in headings)
        {
            var id = WebUtility.HtmlEncode(heading.Id);
            var text = WebUtility.HtmlEncode(heading.Text);
            content.Append($@"<li class=""toc-level-{heading.Level}""><a href=""#{id}"">{text}</a></li>");
        }

        return content.Append("</ul>").ToString();
    }
}
