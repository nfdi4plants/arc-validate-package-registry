using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Namotion.Reflection;
using PackageRegistryService.Models;
using PackageRegistryService.Pages.Components;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace PackageRegistryService.Pages
{
    public static class Packages
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
                                Tags: latestPackage.Tags,
                                Description: latestPackage.Description,
                                LatestVersion: latestPackage.GetSemanticVersionString()
                            );
                        }
                    );

            var content = Layout.Render(
                activeNavbarItem: "",
                title: "ARC validation package registry API",
                content: PackageSummary.RenderList(packageSummaries)
            );

            return TypedResults.Text(content: content, contentType: "text/html");
        }
    }
}
