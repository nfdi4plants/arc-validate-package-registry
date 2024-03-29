﻿// https://www.youtube.com/watch?v=GrJJXixjR8M

namespace PackageRegistryService.Authentication
{
    public class APIKeyEndpointFilter : IEndpointFilter
    {
        private readonly IConfiguration _configuration;

        public APIKeyEndpointFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }
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
