using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;
using PackageRegistryService.Pages.Components;

namespace PackageRegistryService.Pages
{
    public static class Package
    {
        public static async Task<ContentHttpResult> Render(string packageName, ValidationPackageDb database)
        {
            var packages = await 
                database.ValidationPackages
                    .Where(p => p.Name == packageName)
                    .ToArrayAsync();

            var latestPackage =
                packages
                .OrderByDescending(p => p.MajorVersion)
                .ThenByDescending(p => p.MinorVersion)
                .ThenByDescending(p => p.PatchVersion)
                .FirstOrDefault();

            var content = Layout.Render(
                activeNavbarItem: "",
                title: $"Package {packageName} - ARC validation package registry API",
                content: $@"<h1>Validation Package <mark>{packageName}</mark></h1>
<pre>
<code>
arc-validate package install {packageName}

arc-validate package install {packageName} --package-version {latestPackage.GetSemanticVersionString()}
</code>
</pre>
"
            );

            return TypedResults.Text(content: content, contentType: "text/html");
        }
    }
}
