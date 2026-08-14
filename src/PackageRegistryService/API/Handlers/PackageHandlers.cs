using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Models;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Pages.Components;
using PortableSemVer = global::ValidationPackage.Model.SemVer;
using RegistryValidationPackage = PackageRegistryService.Models.ValidationPackage;

namespace PackageRegistryService.API.Handlers
{
    /// <summary>
    /// Handles package-content retrieval and package publication requests.
    /// </summary>
    public class PackageHandlers
    {
        /// <summary>
        /// Gets every validation package, including executable content, and records downloads.
        /// </summary>
        /// <param name="database">The registry database context.</param>
        /// <returns>All packages, or a conflict when stored metadata or content is invalid.</returns>
        public static async Task<Results<Ok<RegistryValidationPackage[]>, Conflict<string>>> GetAllPackages(ValidationPackageDb database)
        {
            var packages = await database.ValidationPackages.ToArrayAsync();

            try
            {
                Array.ForEach(packages, package => package.ToPortableModel());
            }
            catch (ArgumentException error)
            {
                return TypedResults.Conflict($"Internal package metadata is invalid: {error.Message}");
            }

            // Hash validation
            if (packages.Any(p => !ValidationPackageDb.ValidatePackageContent(p, database)))
            {
                return TypedResults.Conflict("Internal package hash collision");
            }

            Array.ForEach(packages, (p => ValidationPackageDb.IncrementDownloadCount(p, database)));
            await database.SaveChangesAsync();

            return TypedResults.Ok(packages);
        }

        /// <summary>
        /// Gets the latest stable version of a package and records its download.
        /// </summary>
        /// <param name="name">The package name.</param>
        /// <param name="database">The registry database context.</param>
        /// <returns>The latest stable package, or an error result when unavailable or invalid.</returns>
        public static async Task<Results<Ok<RegistryValidationPackage>, NotFound<string>, Conflict<string>>> GetLatestPackageByName(string name, ValidationPackageDb database)
        {
            var package = await database.ValidationPackages
                .Where(p => p.Name == name && p.PreReleaseVersionSuffix == "" && p.BuildMetadataVersionSuffix == "") // only serve stable package versions here
                .OrderByDescending(p => p.MajorVersion)
                .ThenByDescending(p => p.MinorVersion)
                .ThenByDescending(p => p.PatchVersion)
                .FirstOrDefaultAsync();

            if (package is null)
            {                 
                return TypedResults.NotFound($"No package '{name}' available.");
            }

            try
            {
                package.ToPortableModel();
            }
            catch (ArgumentException error)
            {
                return TypedResults.Conflict($"Internal package metadata is invalid: {error.Message}");
            }

            if (!ValidationPackageDb.ValidatePackageContent(package, database))
            {
                return TypedResults.Conflict("Internal package hash collision");
            }

            ValidationPackageDb.IncrementDownloadCount(package, database);
            await database.SaveChangesAsync();

            return TypedResults.Ok(package);
        }

        /// <summary>
        /// Gets one exact package version, including executable content, and records its download.
        /// </summary>
        /// <param name="name">The package name.</param>
        /// <param name="version">The full semantic version.</param>
        /// <param name="database">The registry database context.</param>
        /// <returns>The matching package, or an error result for an invalid, missing, or inconsistent package.</returns>
        public static async Task<Results<BadRequest<string>, NotFound<string>, Conflict<string>, Ok<RegistryValidationPackage>>> GetPackageByNameAndVersion(string name, string version, ValidationPackageDb database)
        {
            var semVerOpt = PortableSemVer.tryParse(version);
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

            try
            {
                package.ToPortableModel();
            }
            catch (ArgumentException error)
            {
                return TypedResults.Conflict($"Internal package metadata is invalid: {error.Message}");
            }

            if (!ValidationPackageDb.ValidatePackageContent(package, database))
            {
                return TypedResults.Conflict("Internal package hash collision");
            }

            ValidationPackageDb.IncrementDownloadCount(package, database);
            await database.SaveChangesAsync();

            return TypedResults.Ok(package);
        }

        /// <summary>
        /// Publishes a new package version after validating its metadata and normalized content.
        /// </summary>
        /// <param name="package">The package to publish.</param>
        /// <param name="database">The registry database context.</param>
        /// <returns>The published package, or an error result when it conflicts with existing or invalid state.</returns>
        public static async Task<Results<Ok<RegistryValidationPackage>, Conflict, UnauthorizedHttpResult, UnprocessableEntity<string>>> CreatePackage(RegistryValidationPackage package, ValidationPackageDb database)
        {
            var existing = await database.ValidationPackages.FindAsync(package.Name, package.MajorVersion, package.MinorVersion, package.PatchVersion, package.PreReleaseVersionSuffix, package.BuildMetadataVersionSuffix);
            if (existing != null)
            {
                return TypedResults.Conflict();
            }

            try
            {
                package.ToPortableModel();
            }
            catch (ArgumentException error)
            {
                return TypedResults.UnprocessableEntity(
                    $"Package metadata is invalid: {error.Message}");
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
