using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PackageRegistryService.Models;
using System.Xml.Linq;

namespace PackageRegistryService.API.Handlers
{
    /// <summary>
    /// Handles verification of package content against hashes stored by the registry.
    /// </summary>
    public class VerificationHandlers
    {
        /// <summary>
        /// Verifies that a submitted hash matches a stored package version and its hash record.
        /// </summary>
        /// <param name="hashedPackage">The package identity and hash supplied by the caller.</param>
        /// <param name="database">The registry database context.</param>
        /// <returns>A success, not-found, or hash-mismatch result.</returns>
        public static async Task<Results<Ok, UnprocessableEntity, NotFound>> Verify(PackageContentHash hashedPackage, ValidationPackageDb database)
        {
            var existingHash = await 
                database.Hashes.FindAsync(
                    hashedPackage.PackageName, 
                    hashedPackage.PackageMajorVersion,
                    hashedPackage.PackageMinorVersion,
                    hashedPackage.PackagePatchVersion,
                    hashedPackage.PackagePreReleaseVersionSuffix,
                    hashedPackage.PackageBuildMetadataVersionSuffix
                );

            var existingPackage = await
                database.ValidationPackages.FindAsync(
                    hashedPackage.PackageName,
                    hashedPackage.PackageMajorVersion,
                    hashedPackage.PackageMinorVersion,
                    hashedPackage.PackagePatchVersion,
                    hashedPackage.PackagePreReleaseVersionSuffix,
                    hashedPackage.PackageBuildMetadataVersionSuffix
                );

            if (existingHash is null || existingPackage is null)
            {
                return TypedResults.NotFound();
            }

            if (existingHash.Hash != hashedPackage.Hash)
            {
                return TypedResults.UnprocessableEntity();
            }

            return TypedResults.Ok();
        }

        //public static async Task<Results<Ok<PackageContentHash>, Conflict, UnauthorizedHttpResult>> CreateContentHash(PackageContentHash hashedPackage, ValidationPackageDb database)
        //{

        //    var existing = await 
        //        database.Hashes.FindAsync(
        //            hashedPackage.PackageName,
        //            hashedPackage.PackageMajorVersion,
        //            hashedPackage.PackageMinorVersion,
        //            hashedPackage.PackagePatchVersion
        //        );

        //    if (existing != null)
        //    {
        //        return TypedResults.Conflict();
        //    }

        //    database.Hashes.Add(hashedPackage);
        //    await database.SaveChangesAsync();

        //    return TypedResults.Ok(hashedPackage);

        //}
    }
}
