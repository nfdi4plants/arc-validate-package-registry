using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace PackageRegistryService.API.Handlers
{
    public class VerificationHandlers
    {
        public static async Task<Results<Ok, UnprocessableEntity>> Verify(string name, string version, [FromBody] string hash)
        {
            return TypedResults.UnprocessableEntity();
        }
    }
}
