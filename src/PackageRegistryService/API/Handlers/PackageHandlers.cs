using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Models;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Pages.Components;

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
                .Where(p => p.Name == name) 
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

            var package = await database.ValidationPackages.FindAsync(name, major, minor, revision);

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

        public static async Task<Results<Ok<ValidationPackage>, Conflict, UnauthorizedHttpResult>> CreatePackage(ValidationPackage package, ValidationPackageDb database)
        {
            var existing = await database.ValidationPackages.FindAsync(package.Name, package.MajorVersion, package.MinorVersion, package.PatchVersion);
            if (existing != null)
            {
                return TypedResults.Conflict();
            }

            database.ValidationPackages.Add(package);
            await database.SaveChangesAsync();

            return TypedResults.Ok(package);
        }
    }
}
