using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Models;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Pages.Components;
using AVPRIndex;
using static AVPRIndex.Domain;

namespace PackageRegistryService.API.Handlers
{
    public class PackageHandlers
    {
        // get all validation packages
        public static async Task<Results<Ok<ValidationPackage[]>, Conflict<string>>> GetAllPackages(ValidationPackageDb database)
        {
            var packages = await database.ValidationPackages.ToArrayAsync();

            // Hash validation
            if (packages.Any(p => !ValidationPackageDb.ValidatePackageContent(p, database)))
            {
                return TypedResults.Conflict("Internal package hash collision");
            }

            Array.ForEach(packages, (p => ValidationPackageDb.IncrementDownloadCount(p, database)));
            await database.SaveChangesAsync();

            return TypedResults.Ok(packages);
        }

        public static async Task<Results<Ok<ValidationPackage>, NotFound<string>, Conflict<string>>> GetLatestPackageByName(string name, ValidationPackageDb database)
        {
            var package = await database.ValidationPackages
                .Where(p => p.Name == name && p.BuildMetadataVersionSuffix == "" && p.BuildMetadataVersionSuffix == "") // only serve stable package versions here
                .OrderByDescending(p => p.MajorVersion)
                .ThenByDescending(p => p.MinorVersion)
                .ThenByDescending(p => p.PatchVersion)
                .FirstOrDefaultAsync();

            if (package is null)
            {                 
                return TypedResults.NotFound($"No package '{name}' available.");
            }

            if (!ValidationPackageDb.ValidatePackageContent(package, database))
            {
                return TypedResults.Conflict("Internal package hash collision");
            }

            ValidationPackageDb.IncrementDownloadCount(package, database);
            await database.SaveChangesAsync();

            return TypedResults.Ok(package);
        }

        public static async Task<Results<BadRequest<string>, NotFound<string>, Conflict<string>, Ok<ValidationPackage>>> GetPackageByNameAndVersion(string name, string version, ValidationPackageDb database)
        {
            var semVerOpt =  SemVer.tryParse(version);
            if (semVerOpt is null)
            {
                return TypedResults.BadRequest($"{version} is not a valid semantic version.");
            }
            var semVer = semVerOpt.Value;

            var package = await database.ValidationPackages.FindAsync(name, semVer.Major, semVer.Minor, semVer.Patch, semVer.PreRelease, semVer.BuildMetadata);

            if (package is null)
            {
                return TypedResults.NotFound($"No package '{name}' @ {version} available.");
            }

            if (!ValidationPackageDb.ValidatePackageContent(package, database))
            {
                return TypedResults.Conflict("Internal package hash collision");
            }

            ValidationPackageDb.IncrementDownloadCount(package, database);
            await database.SaveChangesAsync();

            return TypedResults.Ok(package);
        }

        public static async Task<Results<Ok<ValidationPackage>, Conflict, UnauthorizedHttpResult, UnprocessableEntity<string>>> CreatePackage(ValidationPackage package, ValidationPackageDb database)
        {
            var existing = await database.ValidationPackages.FindAsync(package.Name, package.MajorVersion, package.MinorVersion, package.PatchVersion, package.PreReleaseVersionSuffix, package.BuildMetadataVersionSuffix);
            if (existing != null)
            {
                return TypedResults.Conflict();
            }

            if (package.ContentContainsCarriageReturn())
            {
                return TypedResults.UnprocessableEntity("package content contained non-LF line endings");
            }

            ValidationPackageDb.CreatePackageContentHash(package, database);
            database.ValidationPackages.Add(package);
            await database.SaveChangesAsync();

            return TypedResults.Ok(package);
        }
    }
}
