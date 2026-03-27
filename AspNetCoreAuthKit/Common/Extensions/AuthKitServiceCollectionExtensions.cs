using AspNetCoreAuthKit.ApiKey.Handlers;
using AspNetCoreAuthKit.ApiKey.Models;
using AspNetCoreAuthKit.Authorization.Claims;
using AspNetCoreAuthKit.Authorization.Models;
using AspNetCoreAuthKit.Authorization.Policies;
using AspNetCoreAuthKit.JWT;
using AspNetCoreAuthKit.JWT.Interfaces;
using AspNetCoreAuthKit.JWT.Models;
using AspNetCoreAuthKit.Tokens;
using AspNetCoreAuthKit.Tokens.Interfaces;
using AspNetCoreAuthKit.Tokens.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AspNetCoreAuthKit.Common.Extensions
{
    public static class AuthKitServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthKit(
            this IServiceCollection services,
            Action<AuthKitOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            var options = new AuthKitOptions();
            configure(options);

            var authBuilder = services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = options.JwtOptions is not null
                    ? AuthKitDefaults.JwtScheme
                    : AuthKitDefaults.ApiKeyScheme;

                auth.DefaultChallengeScheme = options.JwtOptions is not null
                    ? AuthKitDefaults.JwtScheme
                    : AuthKitDefaults.ApiKeyScheme;
            });

            if (options.JwtOptions is not null)
            {
                var jwtOpts = options.JwtOptions;

                services.AddOptions<JwtAuthOptions>()
                    .Configure(opt =>
                    {
                        opt.SecretKey = jwtOpts.SecretKey;
                        opt.Issuer = jwtOpts.Issuer;
                        opt.Audience = jwtOpts.Audience;
                        opt.AccessTokenExpiry = jwtOpts.AccessTokenExpiry;
                        opt.RefreshTokenExpiry = jwtOpts.RefreshTokenExpiry;
                        opt.Algorithm = jwtOpts.Algorithm;
                        opt.ClockSkew = jwtOpts.ClockSkew;
                    })
                    .Validate(
                        o => !string.IsNullOrWhiteSpace(o.SecretKey),
                        "JwtAuthOptions.SecretKey is required and cannot be empty.")
                    .ValidateOnStart();

                services.AddSingleton<IJwtTokenService, JwtTokenService>();

                authBuilder.AddJwtBearer(bearer =>
                {
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOpts.SecretKey)),
                        ValidateIssuer = !string.IsNullOrEmpty(jwtOpts.Issuer),
                        ValidIssuer = jwtOpts.Issuer,
                        ValidateAudience = !string.IsNullOrEmpty(jwtOpts.Audience),
                        ValidAudience = jwtOpts.Audience,
                        ValidateLifetime = true,
                        ClockSkew = jwtOpts.ClockSkew
                    };
                });
            }

            if (options.ApiKeyOptions is not null)
            {
                var apiKeyOpts = options.ApiKeyOptions;

                services.Configure<ApiKeyOptions>(opt =>
                {
                    opt.HeaderName = apiKeyOpts.HeaderName;
                    opt.QueryParamName = apiKeyOpts.QueryParamName;
                    opt.ValidKeys = apiKeyOpts.ValidKeys;
                    opt.ValidateKeyAsync = apiKeyOpts.ValidateKeyAsync;
                });

                authBuilder.AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(
                    AuthKitDefaults.ApiKeyScheme, _ => { });
            }

            var authzOpts = options.AuthorizationOptions ?? new AuthKitAuthorizationOptions();
            services.AddSingleton(authzOpts);
            services.AddSingleton<IAuthorizationPolicyProvider, DynamicClaimPolicyProvider>();
            services.AddAuthorization(authOptions =>
            {
                CommonPolicies.Register(authOptions, authzOpts);

                if (options.JwtOptions is not null && options.ApiKeyOptions is not null)
                {
                    authOptions.AddPolicy(AuthKitDefaults.PolicyApiKeyOrJwt, policy =>
                        policy
                            .AddAuthenticationSchemes(
                                AuthKitDefaults.JwtScheme,
                                AuthKitDefaults.ApiKeyScheme)
                            .RequireAuthenticatedUser());
                }
            });

            if (options.RefreshTokenOptions is not null)
            {
                var rtOpts = options.RefreshTokenOptions;

                if (rtOpts.CustomStore is not null)
                    services.AddSingleton(typeof(IRefreshTokenStore), rtOpts.CustomStore);
                else
                    services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();

                services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            }

            return services;
        }
    }
}