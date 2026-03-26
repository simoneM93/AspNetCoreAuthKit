using AspNetCoreAuthKit.ApiKey.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AspNetCoreAuthKit.Common
{
    public static class AuthKitDefaults
    {
        public const string JwtScheme = JwtBearerDefaults.AuthenticationScheme;
        public const string ApiKeyScheme = ApiKeyConstants.AuthenticationScheme;

        public const string SubjectClaim = "sub";
        public const string RoleClaim = "role";
        public const string JtiClaim = "jti";

        public const string ApiKeyHeader = "X-Api-Key";

        public const string PolicyAdminOnly = "AuthKit_AdminOnly";
        public const string PolicyAuthenticatedUser = "AuthKit_AuthenticatedUser";
        public const string PolicyApiKeyOrJwt = "AuthKit_ApiKeyOrJwt";
    }
}
