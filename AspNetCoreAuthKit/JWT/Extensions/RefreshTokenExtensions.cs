using AspNetCoreAuthKit.JWT.Models;
using AspNetCoreAuthKit.Tokens;
using AspNetCoreAuthKit.Tokens.Interfaces;
using AspNetCoreAuthKit.Tokens.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreAuthKit.JWT.Extensions
{
    public static class RefreshTokenExtensions
    {
        public static IServiceCollection AddRefreshTokens(
            this IServiceCollection services,
            Action<RefreshTokenStoreOptions>? configure = null)
        {
            var opts = new RefreshTokenStoreOptions();
            configure?.Invoke(opts);

            if (opts.CustomStore is not null)
                services.AddSingleton(typeof(IRefreshTokenStore), opts.CustomStore);
            else
                services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();

            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            return services;
        }
    }
}
