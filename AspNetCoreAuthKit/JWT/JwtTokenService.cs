using AspNetCoreAuthKit.JWT.Interfaces;
using AspNetCoreAuthKit.JWT.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AspNetCoreAuthKit.JWT
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtAuthOptions _options;
        private readonly JwtSecurityTokenHandler _handler = new();

        public JwtTokenService(IOptions<JwtAuthOptions> options)
        {
            _options = options.Value;
            _handler.InboundClaimTypeMap.Clear();
        }

        public TokenResult GenerateToken(TokenRequest request)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            var credentials = new SigningCredentials(key, _options.Algorithm);
            var expiry = DateTimeOffset.UtcNow.Add(request.ExpiresIn ?? _options.AccessTokenExpiry);

            var notBefore = expiry > DateTimeOffset.UtcNow ? DateTime.UtcNow : expiry.UtcDateTime.AddSeconds(-1);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, request.Subject),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };
            claims.AddRange(request.Claims);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: notBefore,
                expires: expiry.UtcDateTime,
                signingCredentials: credentials
            );

            return new TokenResult
            {
                AccessToken = _handler.WriteToken(token),
                RefreshToken = GenerateRefreshToken(),
                ExpiresAt = expiry
            };
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = !string.IsNullOrEmpty(_options.Issuer),
                ValidIssuer = _options.Issuer,
                ValidateAudience = !string.IsNullOrEmpty(_options.Audience),
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = _options.ClockSkew
            };

            try
            {
                return _handler.ValidateToken(token, validationParams, out _);
            }
            catch
            {
                return null;
            }
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal? ValidateTokenIgnoreExpiry(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = !string.IsNullOrEmpty(_options.Issuer),
                ValidIssuer = _options.Issuer,
                ValidateAudience = !string.IsNullOrEmpty(_options.Audience),
                ValidAudience = _options.Audience,
                ValidateLifetime = false,
                ClockSkew = _options.ClockSkew
            };

            try
            {
                return _handler.ValidateToken(token, validationParams, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
