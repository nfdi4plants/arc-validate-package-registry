using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;
using static AVPRIndex.Domain;

namespace PackageRegistryService.API.Handlers
{
    public class DownloadsHandlers
    {
        // get all download stats
        public static async Task<Ok<PackageDownloads[]>> GetAllDownloads(ValidationPackageDb database)
        {
            var downloads = await database.Downloads.ToArrayAsync();
            return TypedResults.Ok(downloads);
        }

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

        public static async Task<Results<BadRequest<string>, NotFound<string>, Ok<PackageDownloads>>> GetDownloadsByNameAndVersion(string name, string version, ValidationPackageDb database)
        {
            var semVerOpt = SemVer.tryParse(version);
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
