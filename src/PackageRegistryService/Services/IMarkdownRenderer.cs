using PackageRegistryService.Models;

namespace PackageRegistryService.Services;

/// <summary>
/// Converts trusted Markdown source into HTML and documentation navigation data.
/// </summary>
public interface IMarkdownRenderer
{
    /// <summary>Renders Markdown as HTML.</summary>
    /// <param name="markdown">The Markdown source.</param>
    /// <returns>The rendered HTML.</returns>
    string Render(string markdown);

    /// <summary>Renders a documentation page and extracts its navigable headings.</summary>
    /// <param name="markdown">The documentation Markdown source.</param>
    /// <returns>The rendered content and heading metadata.</returns>
    RenderedMarkdown RenderDocumentation(string markdown);
}
