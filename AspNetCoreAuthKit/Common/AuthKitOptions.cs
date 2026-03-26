using AspNetCoreAuthKit.ApiKey.Models;
using AspNetCoreAuthKit.Authorization.Models;
using AspNetCoreAuthKit.JWT.Models;

namespace AspNetCoreAuthKit.Common
{
    public class AuthKitOptions
    {
        internal JwtAuthOptions? JwtOptions { get; private set; }
        internal ApiKeyOptions? ApiKeyOptions { get; private set; }
        internal AuthKitAuthorizationOptions? AuthorizationOptions { get; private set; }
        internal RefreshTokenStoreOptions? RefreshTokenOptions { get; private set; }

        public AuthKitOptions UseJwt(Action<JwtAuthOptions> configure)
        {
            JwtOptions = new JwtAuthOptions();
            configure(JwtOptions);
            return this;
        }

        public AuthKitOptions UseApiKey(Action<ApiKeyOptions> configure)
        {
            ApiKeyOptions = new ApiKeyOptions();
            configure(ApiKeyOptions);
            return this;
        }

        public AuthKitOptions UseAuthorization(Action<AuthKitAuthorizationOptions>? configure = null)
        {
            AuthorizationOptions = new AuthKitAuthorizationOptions();
            configure?.Invoke(AuthorizationOptions);
            return this;
        }

        public AuthKitOptions UseRefreshTokens(Action<RefreshTokenStoreOptions>? configure = null)
        {
            RefreshTokenOptions = new RefreshTokenStoreOptions();
            configure?.Invoke(RefreshTokenOptions);
            return this;
        }
    }
}
