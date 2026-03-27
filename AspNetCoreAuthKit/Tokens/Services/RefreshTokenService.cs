using AspNetCoreAuthKit.JWT.Interfaces;
using AspNetCoreAuthKit.JWT.Models;
using AspNetCoreAuthKit.Tokens.Exceptions;
using AspNetCoreAuthKit.Tokens.Interfaces;
using AspNetCoreAuthKit.Tokens.Models;
using Microsoft.Extensions.Options;

namespace AspNetCoreAuthKit.Tokens.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenStore _store;
        private readonly IJwtTokenService _jwtService;
        private readonly JwtAuthOptions _jwtOptions;

        public RefreshTokenService(
            IRefreshTokenStore store,
            IJwtTokenService jwtService,
            IOptions<JwtAuthOptions> jwtOptions)
        {
            ArgumentNullException.ThrowIfNull(store);
            ArgumentNullException.ThrowIfNull(jwtService);
            ArgumentNullException.ThrowIfNull(jwtOptions);

            _store = store;
            _jwtService = jwtService;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<TokenResult> RefreshAsync(
            string expiredAccessToken,
            string refreshToken,
            CancellationToken ct = default)
        {
            var entry = await _store.GetAsync(refreshToken, ct)
                ?? throw new RefreshTokenException("Refresh token non trovato o scaduto.");

            if (!entry.IsValid)
                throw new RefreshTokenException("Refresh token revocato.");

            var principal = _jwtService.ValidateTokenIgnoreExpiry(expiredAccessToken)
                ?? throw new RefreshTokenException("Access token non valido.");

            var subject = principal.FindFirst("sub")?.Value
                ?? throw new RefreshTokenException("Subject mancante nel token.");

            if (subject != entry.Subject)
                throw new RefreshTokenException("Subject non corrisponde.");

            await _store.RevokeAsync(refreshToken, ct);

            var newResult = _jwtService.GenerateToken(new TokenRequest
            {
                Subject = subject,
                Claims = principal.Claims.Where(c =>
                    c.Type != "sub" &&
                    c.Type != "jti" &&
                    c.Type != "iat" &&
                    c.Type != "exp" &&
                    c.Type != "nbf")
            });

            await _store.SaveAsync(newResult.RefreshToken, new RefreshTokenEntry
            {
                Subject = subject,
                ExpiresAt = DateTimeOffset.UtcNow.Add(_jwtOptions.RefreshTokenExpiry),
                Metadata = entry.Metadata
            }, ct);

            return newResult;
        }

        public Task RevokeAsync(string refreshToken, CancellationToken ct = default)
            => _store.RevokeAsync(refreshToken, ct);

        public Task RevokeAllAsync(string subject, CancellationToken ct = default)
            => _store.RevokeAllAsync(subject, ct);
    }
}