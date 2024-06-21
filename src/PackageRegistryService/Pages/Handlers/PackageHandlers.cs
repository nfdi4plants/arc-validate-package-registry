using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;
using PackageRegistryService.Pages.Components;
using System.Text;
using System.Xml.Linq;
using static AVPRIndex.Domain;

namespace PackageRegistryService.Pages.Handlers
{
    public static class PackageHandlers
    {
        public static async Task<Results<ContentHttpResult, NotFound, BadRequest<string>>> Render(string packageName, string version, ValidationPackageDb database)
        {
            var semVerOpt = SemVer.tryParse(version);
            if (semVerOpt is null)
            {
                return TypedResults.BadRequest($"{version} is not a valid semantic version.");
            }
            var semVer = semVerOpt.Value;

            var package = await database.ValidationPackages.FindAsync(packageName, semVer.Major, semVer.Minor, semVer.Patch, semVer.PreRelease, semVer.BuildMetadata);
            var downloads = await database.Downloads.FindAsync(packageName, semVer.Major, semVer.Minor, semVer.Patch, semVer.PreRelease, semVer.BuildMetadata);

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
                    packageSummary: package.Summary,
                    packageDescription: package.Description,
                    packageReleaseNotes: package.ReleaseNotes ?? "",
                    packageAuthors: (package.Authors ?? []).ToArray(),
                    versionTable: Components.PackageAvailableVersion.RenderVersionTable(packages),
                    downloads: downloads?.Downloads ?? 0
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
                .Where(p => p.BuildMetadataVersionSuffix == "" && p.BuildMetadataVersionSuffix == "")
                .OrderByDescending(p => p.MajorVersion)
                .ThenByDescending(p => p.MinorVersion)
                .ThenByDescending(p => p.PatchVersion)
                .FirstOrDefault();

            if (latestPackage == null)
            {
                return TypedResults.NotFound();
            }

            var downloads = await database.Downloads.FindAsync(latestPackage.Name, latestPackage.MajorVersion, latestPackage.MinorVersion, latestPackage.PatchVersion, latestPackage.PreReleaseVersionSuffix, latestPackage.BuildMetadataVersionSuffix);

            var page = Layout.Render(
                activeNavbarItem: "",
                title: $"Package {packageName} - ARC validation package registry API",
                content: Components.Package.Render(
                    packageName: packageName,
                    packageVersion: latestPackage.GetSemanticVersionString(),
                    packageContent: Encoding.UTF8.GetString(latestPackage.PackageContent),
                    packageReleaseDate: latestPackage.ReleaseDate,
                    packageTags: (latestPackage.Tags ?? []).Select(t => t.Name).ToArray(),
                    packageSummary: latestPackage.Summary,
                    packageDescription: latestPackage.Description,
                    packageReleaseNotes: latestPackage.ReleaseNotes ?? "",
                    packageAuthors: (latestPackage.Authors ?? []).ToArray(),
                    versionTable: Components.PackageAvailableVersion.RenderVersionTable(packages),
                    downloads: downloads?.Downloads ?? 0
                )
            );

            return TypedResults.Text(content: page, contentType: "text/html");
        }
    }
}
