using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Models;
using PackageRegistryService.Pages.Components;

namespace PackageRegistryService.Pages.Handlers
{
    /// <summary>
    /// Handles the registry website's landing page.
    /// </summary>
    public static class IndexHandlers
    {
        /// <summary>Renders the landing page within the shared website layout.</summary>
        public static async Task<ContentHttpResult> Render()
        {

            var content =
                Layout.Render(
                    activeNavbarItem: "Home",
                    title: "AVPR: ARC validation package registry",
                    content: Components.Index.Render()
                );

            return TypedResults.Text(content: content, contentType: "text/html");

        }
    }

}
