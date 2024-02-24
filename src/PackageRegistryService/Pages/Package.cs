using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;
using PackageRegistryService.Pages.Components;
using System.Text;

namespace PackageRegistryService.Pages
{
    public static class Package
    {
        public static async Task<Results<ContentHttpResult, NotFound>> Render(string packageName, string version, ValidationPackageDb database)
        {
            var content = $"{packageName}, {version}";
            return TypedResults.Text(content: content, contentType: "text/html");
        }
        public static async Task<Results<ContentHttpResult, NotFound>> RenderLatest(string packageName, ValidationPackageDb database)
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

            if (latestPackage == null)
            {
                return TypedResults.NotFound();
            }

            var content = $@"<section>
  <hgroup>
    <h1>Validation Package <mark>{packageName}</mark></h1>
    <p>{PackageTag.RenderAllInline(latestPackage.Tags)}</p>
    <p>v{latestPackage.GetSemanticVersionString()}</p>
  </hgroup>
</section>
<hr />
<section>
  <h4>Install with <a href=""a"">arc-validate</a></h3>
  <pre><code> arc-validate package install {packageName} --package-version {latestPackage.GetSemanticVersionString()}</code></pre>
</section>
<hr />
<section> 
  <h2>Description</h2>
  {PackageDescription.Render(latestPackage.Description)}
</section>
<hr />
<section>
  <details>
    <summary role=""button"" class=""primary"">Release notes</summary>
      {PackageReleaseNotes.Render(latestPackage.ReleaseNotes)}
  </details>
  <hr />
  <details>
    <summary role=""button"" class=""primary"">Browse code (v{latestPackage.GetSemanticVersionString()})</summary>
      <pre><code>{Encoding.UTF8.GetString(latestPackage.PackageContent)}</code></pre>
  </details>
  <hr />
  <details>
    <summary role=""button"" class=""primary"">Available versions</summary>
    {PackageAvailableVersion.RenderVersionTable(packages)}
  </details>
</section>
<hr />
";

            var page = Layout.Render(
                activeNavbarItem: "",
                title: $"Package {packageName} - ARC validation package registry API",
                content: content
            );

            return TypedResults.Text(content: page, contentType: "text/html");
        }
    }
}
