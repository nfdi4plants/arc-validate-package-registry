using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PackageRegistryService.Models;
using System.Xml.Linq;

namespace PackageRegistryService.API.Handlers
{
    public class VerificationHandlers
    {
        public static async Task<Results<Ok, UnprocessableEntity, NotFound>> Verify(PackageContentHash hashedPackage, ValidationPackageDb database)
        {
            var package = await 
                database.Hashes.FindAsync(
                    hashedPackage.PackageName, 
                    hashedPackage.PackageMajorVersion,
                    hashedPackage.PackageMinorVersion,
                    hashedPackage.PackagePatchVersion
                );

            if (package is null)
            {
                return TypedResults.NotFound();
            }

            if (package.Hash != hashedPackage.Hash)
            {
                return TypedResults.UnprocessableEntity();
            }

            return TypedResults.Ok();
        }
    }
}
