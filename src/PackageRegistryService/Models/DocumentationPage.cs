namespace PackageRegistryService.Models;

/// <summary>
/// Contains one rendered documentation page and the headings used for navigation.
/// </summary>
/// <param name="Title">The page title.</param>
/// <param name="Html">The rendered documentation HTML.</param>
/// <param name="Headings">The navigable headings extracted from the document.</param>
public sealed record DocumentationPage(
    string Title,
    string Html,
    IReadOnlyList<MarkdownHeading> Headings
);
