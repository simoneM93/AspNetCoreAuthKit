using AspNetCoreAuthKit.Authorization.Models;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCoreAuthKit.Authorization.Policies
{
    public static class CommonPolicies
    {
        public const string AdminOnly = "AuthKit_AdminOnly";
        public const string AuthenticatedUser = "AuthKit_AuthenticatedUser";
        public const string ApiKeyOrJwt = "AuthKit_ApiKeyOrJwt";

        internal static void Register(AuthorizationOptions options, AuthKitAuthorizationOptions authKitOpts)
        {
            options.AddPolicy(AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(AdminOnly, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(authKitOpts.RoleClaimType, authKitOpts.AdminRoleValue));

            options.AddPolicy(ApiKeyOrJwt, policy =>
                policy
                    .AddAuthenticationSchemes(
                        authKitOpts.JwtScheme,
                        authKitOpts.ApiKeyScheme)
                    .RequireAuthenticatedUser());
        }
    }
}
