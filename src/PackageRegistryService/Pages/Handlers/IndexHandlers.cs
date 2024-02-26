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
                    activeNavbarItem: "",
                    title: "ARC validation package registry API",
                    content: @"<h1>ARC validation package registry API</h1><br></br>
<p>This service provides a browser and API for ARC validation packages.</p>
<p><a href=""/packages"">Browse all available packages</a></p>
<p>For <b>programmatic access</b>, go checkout the <a href=""swagger"">API documentation</a>");

            return TypedResults.Text(content: content, contentType: "text/html");

        }
    }

}