using PackageRegistryService.Models;

namespace PackageRegistryService.Services;

/// <summary>
/// Provides rendered repository documentation by safe document-relative path.
/// </summary>
public interface IDocumentationProvider
{
    /// <summary>
    /// Loads and renders one documentation page.
    /// </summary>
    /// <param name="document">The documentation-relative path.</param>
    /// <returns>The rendered page, or <see langword="null"/> when the path is invalid or unavailable.</returns>
    DocumentationPage? GetPage(string document);
}
