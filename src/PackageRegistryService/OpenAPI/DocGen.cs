using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;

namespace PackageRegistryService.OpenAPI
{
    public class DocGen
    {
        public static void GeneratorSetup (AspNetCoreOpenApiDocumentGeneratorSettings settings)
        {
            settings.Title = "ARC validation package registry API";
            settings.Version = "v1";
            settings.Description = "A simple API for retrieving ARC validation packages";

            // add a security definition for API key authentication
            settings.AddSecurity(
                name: "ApiKey",
                new NSwag.OpenApiSecurityScheme
                {
                    Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                    Name = "X-API-KEY",
                    In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                    Description = "API Key required to access POST endpoints"
                }
            );

            // manually change security for selected endpoints, was not able to find a more automated way for minimal APIs with NSwag and this exact setup. 
            // For e.g. JWT, this would work because we could rely on full blown authorization service/config, but this is a simple API key checked via endpoint filters
            settings.OperationProcessors.Add(
                new OperationSecurityProcessor(
                    secureEndpointIds: ["CreatePackage", "CreatePackageContentHash"]
                )
            );
            // fix for WithDescription and WithSummary methods not working with nswag and minimal API endpoints
            settings.OperationProcessors.Add(new OperationMetadataProcessor());

            // only inclide "api" paths in docs
            settings.OperationProcessors.Add(new OperationFilterProcessor());
        }
    }
}
