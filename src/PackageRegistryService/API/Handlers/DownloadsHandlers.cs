using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;

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

            var downloads = await database.Downloads.FindAsync(name, major, minor, revision);

            return downloads is null
                ? TypedResults.NotFound($"No download stats for package '{name}' version '{version}' available.")
                : TypedResults.Ok(downloads);
        }
    }
}
