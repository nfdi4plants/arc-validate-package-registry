using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Models;
using PackageRegistryService.Pages.Components;

namespace PackageRegistryService.Pages.Handlers
{
    public static class AboutHandlers
    {
        public static async Task<ContentHttpResult> Render()
        {

            var content =
                Layout.Render(
                    activeNavbarItem: "About",
                    title: "ARC validation package registry API",
                    content: About.Render()
                );

            return TypedResults.Text(content: content, contentType: "text/html");

        }
    }

}