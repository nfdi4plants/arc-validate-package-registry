using Microsoft.AspNetCore.Http.HttpResults;

namespace PackageRegistryService.Pages.Handlers;

/// <summary>
/// Handles the legacy about-page route.
/// </summary>
public static class AboutHandlers
{
    /// <summary>Redirects visitors to the canonical About section in the documentation.</summary>
    public static RedirectHttpResult Render() =>
        TypedResults.Redirect("/docs/index.md#about-avpr", permanent: true);
}
