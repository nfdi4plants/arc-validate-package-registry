namespace PackageRegistryService.Authentication
{
    /// <summary>
    /// Names the HTTP and configuration keys used by API-key authentication.
    /// </summary>
    public class AuthConstants
    {
        /// <summary>The request header that carries the API key.</summary>
        public const string APIKeyHeaderName = "X-API-KEY";

        /// <summary>The configuration path containing the expected API key.</summary>
        public const string ApiKeySectionName = "Authentication:ApiKey";
    }
}
