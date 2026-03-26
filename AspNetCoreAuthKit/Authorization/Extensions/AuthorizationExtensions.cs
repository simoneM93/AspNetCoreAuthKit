using AspNetCoreAuthKit.Authorization.Claims;
using AspNetCoreAuthKit.Authorization.Models;
using AspNetCoreAuthKit.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreAuthKit.Authorization.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAuthKitAuthorization(
            this IServiceCollection services,
            Action<AuthKitAuthorizationOptions>? configure = null)
        {
            var opts = new AuthKitAuthorizationOptions();
            configure?.Invoke(opts);
            services.AddSingleton(opts);

            // Registra il provider dinamico per [RequireClaim]
            services.AddSingleton<IAuthorizationPolicyProvider, DynamicClaimPolicyProvider>();

            services.AddAuthorization(authOpts =>
            {
                CommonPolicies.Register(authOpts, opts);
            });

            return services;
        }
    }
}
