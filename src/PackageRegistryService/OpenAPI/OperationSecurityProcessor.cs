using Microsoft.AspNetCore.Authorization;
using NSwag.Generation.Processors.Contexts;
using NSwag.Generation.Processors;
using NSwag;

namespace PackageRegistryService.OpenAPI
{
    /// <summary>
    /// Applies API-key security requirements to selected operation identifiers.
    /// </summary>
    /// <param name="secureEndpointIds">The operation identifiers that require an API key.</param>
    public class OperationSecurityProcessor(string[] secureEndpointIds) : IOperationProcessor
    {

        /// <summary>
        /// Adds the configured security requirement to matching generated operations.
        /// </summary>
        /// <param name="operationProcessorContext">The operation generation context.</param>
        /// <returns><see langword="true"/> so the processed operation remains in the document.</returns>
        public bool Process(OperationProcessorContext operationProcessorContext)
        {
            foreach (OpenApiOperationDescription operationDescription in operationProcessorContext.AllOperationDescriptions)
            {
                if (secureEndpointIds.Contains(operationDescription.Operation.OperationId))
                {
                    operationDescription.Operation.Security = new OpenApiSecurityRequirement[]
                    {
                        new OpenApiSecurityRequirement
                        {
                            {
                                "ApiKey", new string[] { }
                            }
                        }

                    };
                }

            }

            return true;
        }

    }
}
