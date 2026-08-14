using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Pages.Components;
using PackageRegistryService.Services;

namespace PackageRegistryService.Pages.Handlers;

/// <summary>
/// Handles documentation index and document-page requests.
/// </summary>
public static class DocumentationHandlers
{
    /// <summary>Redirects the documentation root to its canonical index document.</summary>
    public static RedirectHttpResult RenderIndex() =>
        TypedResults.Redirect("/docs/index.md", permanent: false);

    /// <summary>
    /// Loads and renders one repository documentation page.
    /// </summary>
    /// <param name="document">The documentation-relative path.</param>
    /// <param name="documentation">The documentation content provider.</param>
    /// <returns>The rendered HTML page, or a not-found result.</returns>
    public static Results<ContentHttpResult, NotFound> Render(
        string document,
        IDocumentationProvider documentation
    )
    {
        var page = documentation.GetPage(document);
        if (page is null)
        {
            return TypedResults.NotFound();
        }

        var content = Layout.Render(
            activeNavbarItem: "Documentation",
            title: Documentation.RenderTitle(page),
            content: Documentation.Render(page),
            additionalHeadContent: @"<link rel=""stylesheet"" href=""/css/documentation.css"" />"
        );

        return TypedResults.Text(content: content, contentType: "text/html");
    }
}
