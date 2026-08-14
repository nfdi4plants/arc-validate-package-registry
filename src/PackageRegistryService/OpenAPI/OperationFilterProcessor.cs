using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace PackageRegistryService.OpenAPI
{
    /// <summary>
    /// Limits the OpenAPI document to programmatic API routes.
    /// </summary>
    public class OperationFilterProcessor : IOperationProcessor
    {
        /// <summary>
        /// Includes an operation only when its path is under the API route prefix.
        /// </summary>
        /// <param name="operationProcessorContext">The operation generation context.</param>
        /// <returns><see langword="true"/> for API routes; otherwise <see langword="false"/>.</returns>
        public bool Process(OperationProcessorContext operationProcessorContext)
        {
            return operationProcessorContext.OperationDescription.Path.StartsWith("/api/");
        }
    }
}
