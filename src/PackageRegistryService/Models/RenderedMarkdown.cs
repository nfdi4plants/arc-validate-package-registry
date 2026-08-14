namespace PackageRegistryService.Models;

/// <summary>
/// Identifies a rendered Markdown heading used to build page navigation.
/// </summary>
/// <param name="Id">The generated HTML anchor identifier.</param>
/// <param name="Text">The plain-text heading label.</param>
/// <param name="Level">The Markdown heading level.</param>
public sealed record MarkdownHeading(string Id, string Text, int Level);

/// <summary>
/// Contains rendered Markdown HTML and the headings extracted during rendering.
/// </summary>
/// <param name="Html">The rendered HTML.</param>
/// <param name="Headings">The navigable document headings.</param>
public sealed record RenderedMarkdown(
    string Html,
    IReadOnlyList<MarkdownHeading> Headings
);
