using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;
using PackageRegistryService.Pages.Components;
using System.Text;
using System.Xml.Linq;

namespace PackageRegistryService.Pages.Handlers
{
    public static class PackageHandlers
    {
        public static async Task<Results<ContentHttpResult, NotFound, BadRequest<string>>> Render(string packageName, string version, ValidationPackageDb database)
        {
            var splt = version.Split('.');
            if (splt.Length != 3)
            {
                return TypedResults.BadRequest("version was not a of valid format MAJOR.MINOR.REVISION");
            }

            int major; int minor; int revision;

            if (
                !int.TryParse(splt[0], out major)
                || !int.TryParse(splt[1], out minor)
                || !int.TryParse(splt[2], out revision)
            )
            {
                return TypedResults.BadRequest("version was not a of valid format MAJOR.MINOR.REVISION");
            }

            var package = await database.ValidationPackages.FindAsync(packageName, major, minor, revision);

            if (package == null)
            {
                return TypedResults.NotFound();
            }

            var packages = await 
                database.ValidationPackages
                .Where(p => p.Name == packageName)
                .ToArrayAsync();

            var page = Layout.Render(
                activeNavbarItem: "",
                title: $"Package {packageName} - ARC validation package registry API",
                content: Components.Package.Render(
                    packageName: packageName,
                    packageVersion: package.GetSemanticVersionString(),
                    packageContent: Encoding.UTF8.GetString(package.PackageContent),
                    packageReleaseDate: package.ReleaseDate,
                    packageTags: (package.Tags ?? []).Select(t => t.Name).ToArray(),
                    packageDescription: package.Description,
                    packageReleaseNotes: package.ReleaseNotes ?? "",
                    packageAuthors: (package.Authors ?? []).ToArray(),
                    versionTable: Components.PackageAvailableVersion.RenderVersionTable(packages)
                )
            );

            return TypedResults.Text(content: page, contentType: "text/html");
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

            var page = Layout.Render(
                activeNavbarItem: "",
                title: $"Package {packageName} - ARC validation package registry API",
                content: Components.Package.Render(
                    packageName: packageName,
                    packageVersion: latestPackage.GetSemanticVersionString(),
                    packageContent: Encoding.UTF8.GetString(latestPackage.PackageContent),
                    packageReleaseDate: latestPackage.ReleaseDate,
                    packageTags: (latestPackage.Tags ?? []).Select(t => t.Name).ToArray(),
                    packageDescription: latestPackage.Description,
                    packageReleaseNotes: latestPackage.ReleaseNotes ?? "",
                    packageAuthors: (latestPackage.Authors ?? []).ToArray(),
                    versionTable: Components.PackageAvailableVersion.RenderVersionTable(packages)
                )
            );

            return TypedResults.Text(content: page, contentType: "text/html");
        }
    }
}
