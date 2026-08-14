using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Pages.Components;
using PackageRegistryService.Services;

namespace PackageRegistryService.Pages.Handlers;

/// <summary>
/// Handles the service releases page.
/// </summary>
public static class ReleaseHandlers
{
    /// <summary>
    /// Renders current build provenance and release notes.
    /// </summary>
    /// <param name="releaseInfo">The current service release information.</param>
    /// <returns>The rendered releases page.</returns>
    public static ContentHttpResult Render(IServiceReleaseInfoProvider releaseInfo)
    {
        var content = Layout.Render(
            activeNavbarItem: "Releases",
            title: "AVPR service releases",
            content: Releases.Render(releaseInfo.Current, releaseInfo.ReleaseNotesHtml)
        );

        return TypedResults.Text(content: content, contentType: "text/html");
    }
}
