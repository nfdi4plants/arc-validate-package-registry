// https://www.youtube.com/watch?v=GrJJXixjR8M

namespace PackageRegistryService.Authentication
{
    /// <summary>
    /// Rejects requests whose configured API-key header does not match the service secret.
    /// </summary>
    public class APIKeyEndpointFilter : IEndpointFilter
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Creates an API-key filter backed by application configuration.
        /// </summary>
        /// <param name="configuration">The service configuration containing the expected key.</param>
        public APIKeyEndpointFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// Authorizes the current request before invoking the protected endpoint.
        /// </summary>
        /// <param name="context">The current endpoint-filter invocation.</param>
        /// <param name="next">The next filter or endpoint delegate.</param>
        /// <returns>The downstream result when authorized; otherwise an unauthorized result.</returns>
        public async ValueTask<object> InvokeAsync(
            EndpointFilterInvocationContext context,  
            EndpointFilterDelegate next
        )
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(AuthConstants.APIKeyHeaderName, out var extractedApiKey))
            {
                return TypedResults.Unauthorized();
            }

            var apiKey = _configuration.GetValue<string>(AuthConstants.ApiKeySectionName);

            if (!apiKey.Equals(extractedApiKey))
            {
                return TypedResults.Unauthorized();
            }

            return await next(context);
        }
    }
}
