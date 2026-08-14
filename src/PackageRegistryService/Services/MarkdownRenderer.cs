using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using PackageRegistryService.Models;

namespace PackageRegistryService.Services;

/// <summary>
/// Renders Markdown with the service's shared Markdig feature set.
/// </summary>
public sealed class MarkdownRenderer : IMarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    /// <inheritdoc />
    public string Render(string markdown) => Markdown.ToHtml(markdown, _pipeline);

    /// <inheritdoc />
    public RenderedMarkdown RenderDocumentation(string markdown)
    {
        var document = Markdown.Parse(markdown, _pipeline);
        var headings = document
            .Descendants<HeadingBlock>()
            .Where(heading => heading.Level is 2 or 3)
            .Select(heading => new MarkdownHeading(
                heading.GetAttributes().Id ?? string.Empty,
                GetHeadingText(markdown, heading),
                heading.Level
            ))
            .Where(heading =>
                heading.Id.Length > 0
                && !heading.Text.Equals("Contents", StringComparison.OrdinalIgnoreCase)
            )
            .ToArray();

        return new RenderedMarkdown(
            Markdown.ToHtml(RemoveInlineContents(markdown, document), _pipeline),
            headings
        );
    }

    /// <summary>Extracts the plain-text label from one parsed heading.</summary>
    private string GetHeadingText(string markdown, HeadingBlock heading)
    {
        var span = heading.Inline!.Span;
        var headingMarkdown = markdown.Substring(span.Start, span.End - span.Start + 1);
        return Markdown.ToPlainText(headingMarkdown, _pipeline).Trim();
    }

    /// <summary>
    /// Removes a source document's inline contents list because page navigation replaces it.
    /// </summary>
    private static string RemoveInlineContents(
        string markdown,
        MarkdownDocument document
    )
    {
        for (var index = 0; index < document.Count - 1; index++)
        {
            if (
                document[index] is not HeadingBlock { Level: 2 } heading
                || document[index + 1] is not ListBlock contentsList
            )
            {
                continue;
            }

            var span = heading.Inline!.Span;
            var headingText = markdown.Substring(span.Start, span.End - span.Start + 1);
            if (!headingText.Equals("Contents", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var start = heading.Span.Start;
            var end = contentsList.Span.End;
            return markdown.Remove(start, end - start + 1);
        }

        return markdown;
    }
}
