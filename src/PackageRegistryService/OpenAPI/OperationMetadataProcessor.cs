using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace PackageRegistryService.OpenAPI
{
    public class OperationMetadataProcessor : IOperationProcessor
    {
        public Dictionary<string, Dictionary<string,string>> EndpointMetadata = new Dictionary<string, Dictionary<string,string>>
        {
            {
                "CreatePackage", new Dictionary<string, string>
                {
                    { "Summary", "Submit a new validation package" },
                    { "Description", "Submit a new validation package to the package registry. This Endpoint requires API Key authentication." }
                }
            },
            {
                "GetAllPackages", new Dictionary<string, string>
                {
                    { "Summary", "Get all validation packages" },
                    { "Description", "Get all validation packages from the package registry. Note that this endpoint returns all versions of each package. Package content is a base64 encoded byte array containing the package executable." }
                }
            },
            {
                "GetLatestPackageByName", new Dictionary<string, string>
                {
                    { "Summary", "Get the latest version of a validation package" },
                    { "Description", "Get the latest version of a validation package from the package registry. Package content is a base64 encoded byte array containing the package executable." }
                }
            },
            {
                "GetPackageByNameAndVersion", new Dictionary<string, string>
                {
                    { "Summary", "Get a specific version of a validation package" },
                    { "Description", "Get a specific version of a validation package from the package registry. Package content is a base64 encoded byte array containing the package executable." }
                }
            },
            {
                "VerifyPackageContent", new Dictionary<string, string>
                {
                    { "Summary", "Verify a content hash for a given package." },
                    { "Description", "Verify a content hash for a given package. Hashes are MD5 hex fingerprints." }
                }
            },
            //{
            //    "CreatePackageContentHash", new Dictionary<string, string>
            //    {
            //        { "Summary", "Create a content hash for a given package." },
            //        { "Description", "Create a content hash for a given package. Hashes are MD5 hex fingerprints. This Endpoint requires API Key authentication." }
            //    }
            //},
            {
                "GetAllDownloads", new Dictionary<string, string>
                {
                    { "Summary", "Get all download statistics" },
                    { "Description", "Get all download statistics for all validation packages." }
                }
            },
            {
                "GetAllDownloadsByName", new Dictionary<string, string>
                {
                    { "Summary", "Get download statistics for all versions specific package" },
                    { "Description", "Get download statistics for all versions specific package" }
                }
            },
            {
                "GetDownloadsByNameAndVersion", new Dictionary<string, string>
                {
                    { "Summary", "Get download statistics for a specific version of a specific package" },
                    { "Description", "Get download statistics for a specific version of a specific package" }
                }
            }
        };

        public bool Process(OperationProcessorContext operationProcessorContext)
        {
            foreach (OpenApiOperationDescription operationDescription in operationProcessorContext.AllOperationDescriptions)
            {
                var op = operationDescription.Operation;
                if (EndpointMetadata.ContainsKey(op.OperationId)) {
                    op.Summary = EndpointMetadata[op.OperationId]["Summary"];
                    op.Description = EndpointMetadata[op.OperationId]["Description"];
                }
            }

            return true;
        }

    }
}
