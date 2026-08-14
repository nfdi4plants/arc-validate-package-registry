using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;
using PortableSemVer = global::ValidationPackage.Model.SemVer;

namespace PackageRegistryService.API.Handlers
{
    /// <summary>
    /// Handles read-only requests for validation-package download statistics.
    /// </summary>
    public class DownloadsHandlers
    {
        /// <summary>
        /// Gets download statistics for every published package version.
        /// </summary>
        /// <param name="database">The registry database context.</param>
        /// <returns>An HTTP result containing all download counters.</returns>
        public static async Task<Ok<PackageDownloads[]>> GetAllDownloads(ValidationPackageDb database)
        {
            var downloads = await database.Downloads.ToArrayAsync();
            return TypedResults.Ok(downloads);
        }

        /// <summary>
        /// Gets download statistics for all published versions of a package.
        /// </summary>
        /// <param name="name">The package name.</param>
        /// <param name="database">The registry database context.</param>
        /// <returns>The matching counters, or a not-found result when none exist.</returns>
        public static async Task<Results<Ok<PackageDownloads[]>, NotFound<string>>> GetAllDownloadsByName(string name, ValidationPackageDb database)
        {
            var downloads =
                await database.Downloads
                    .Where(p => p.PackageName == name)
                    .ToArrayAsync();

            return downloads is null || downloads.Length == 0
                ? TypedResults.NotFound($"No download stats for package '{name}' available.")
                : TypedResults.Ok(downloads);
        }

        /// <summary>
        /// Gets the download statistics for one exact package version.
        /// </summary>
        /// <param name="name">The package name.</param>
        /// <param name="version">The full semantic version.</param>
        /// <param name="database">The registry database context.</param>
        /// <returns>The counter, or an error result for an invalid or unknown version.</returns>
        public static async Task<Results<BadRequest<string>, NotFound<string>, Ok<PackageDownloads>>> GetDownloadsByNameAndVersion(string name, string version, ValidationPackageDb database)
        {
            var semVerOpt = PortableSemVer.tryParse(version);
            if (semVerOpt is null)
            {
                return TypedResults.BadRequest($"{version} is not a valid semantic version.");
            }
            var semVer = semVerOpt.Value;

            var downloads = await database.Downloads.FindAsync(name, semVer.Major, semVer.Minor, semVer.Patch, semVer.PreRelease, semVer.BuildMetadata);

            return downloads is null
                ? TypedResults.NotFound($"No download stats for package '{name}' version '{version}' available.")
                : TypedResults.Ok(downloads);
        }
    }
}
