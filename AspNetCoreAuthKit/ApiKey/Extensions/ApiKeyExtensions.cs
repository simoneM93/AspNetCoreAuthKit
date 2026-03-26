using AspNetCoreAuthKit.ApiKey.Constants;
using AspNetCoreAuthKit.ApiKey.Handlers;
using AspNetCoreAuthKit.ApiKey.Models;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreAuthKit.ApiKey.Extensions
{
    public static class ApiKeyExtensions
    {
        public static IServiceCollection AddApiKeyAuthentication(
            this IServiceCollection services,
            Action<ApiKeyOptions> configure)
        {
            services.Configure(configure);

            services
                .AddAuthentication(ApiKeyConstants.AuthenticationScheme)
                .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(
                    ApiKeyConstants.AuthenticationScheme,
                    _ => { }
                );

            return services;
        }
    }
}
