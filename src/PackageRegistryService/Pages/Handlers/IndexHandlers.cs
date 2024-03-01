using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Models;
using PackageRegistryService.Pages.Components;

namespace PackageRegistryService.Pages.Handlers
{
    public static class IndexHandlers
    {
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