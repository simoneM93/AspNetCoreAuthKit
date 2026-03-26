using AspNetCoreAuthKit.JWT.Interfaces;
using AspNetCoreAuthKit.JWT.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace AspNetCoreAuthKit.JWT.Extensions
{
    public static class JwtAuthExtensions
    {
        public static IServiceCollection AddSimpleJwtBearer(
            this IServiceCollection services,
            Action<JwtAuthOptions> configure)
        {
            services.Configure(configure);
            services.AddSingleton<IJwtTokenService, JwtTokenService>();

            var options = new JwtAuthOptions();
            configure(options);

            services
                .AddAuthentication(auth =>
                {
                    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(bearer =>
                {
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(options.SecretKey)),
                        ValidateIssuer = !string.IsNullOrEmpty(options.Issuer),
                        ValidIssuer = options.Issuer,
                        ValidateAudience = !string.IsNullOrEmpty(options.Audience),
                        ValidAudience = options.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });

            return services;
        }
    }
}
