using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Namotion.Reflection;
using PackageRegistryService.Models;
using PackageRegistryService.Pages.Components;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace PackageRegistryService.Pages.Handlers
{
    public static class PackagesHandlers
    {
        public static async Task<ContentHttpResult> Render(ValidationPackageDb database)
        {
            var packages = await database.ValidationPackages.ToArrayAsync();

            var latestPackages =
                packages
                .OrderByDescending(p => p.MajorVersion)
                .ThenByDescending(p => p.MinorVersion)
                .ThenByDescending(p => p.PatchVersion)
                .FirstOrDefault();

            var packageSummaries =
                packages
                    .GroupBy(p => p.Name)
                    .ToList()
                    .Select(group =>
                        {
                            var latestPackage =
                                group
                                    .OrderByDescending(p => p.MajorVersion)
                                    .ThenByDescending(p => p.MinorVersion)
                                    .ThenByDescending(p => p.PatchVersion)
                                    .FirstOrDefault();
                            return
                            new PackageSummary(
                                Name: group.Key,
                                Tags: (latestPackage.Tags ?? []).Select(t => t.Name).ToArray(),
                                Description: latestPackage.Description,
                                ReleaseDate: latestPackage.ReleaseDate,
                                LatestVersion: latestPackage.GetSemanticVersionString()
                            );
                        }
                    );

            var content = Layout.Render(
                activeNavbarItem: "Browse Packages",
                title: "ARC validation package registry API",
                content: PackageSummary.RenderList(packageSummaries)
            );

            return TypedResults.Text(content: content, contentType: "text/html");
        }
    }
}
