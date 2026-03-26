using AspNetCoreAuthKit.JWT;
using AspNetCoreAuthKit.JWT.Models;
using AspNetCoreAuthKit.Tokens;
using AspNetCoreAuthKit.Tokens.Exceptions;
using AspNetCoreAuthKit.Tokens.Models;
using AspNetCoreAuthKit.Tokens.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AspNetCoreAuthKit.Tests.Tokens
{
    public class RefreshTokenServiceTests
    {
        private readonly JwtTokenService _jwtService;
        private readonly InMemoryRefreshTokenStore _store;
        private readonly RefreshTokenService _sut;

        public RefreshTokenServiceTests()
        {
            var options = new JwtAuthOptions
            {
                SecretKey = "super-secret-key-long-enough-for-hmac-256!",
                Issuer = "test-issuer",
                Audience = "test-audience",
                AccessTokenExpiry = TimeSpan.FromMinutes(15),
                RefreshTokenExpiry = TimeSpan.FromDays(7)
            };

            _jwtService = new JwtTokenService(Options.Create(options));
            _store = new InMemoryRefreshTokenStore();
            _sut = new RefreshTokenService(_store, _jwtService, Options.Create(options));
        }

        private async Task<(string AccessToken, string RefreshToken)> LoginAsync(string subject = "user-123")
        {
            var result = _jwtService.GenerateToken(new TokenRequest
            {
                Subject = subject,
                Claims = [new Claim("role", "admin")]
            });

            await _store.SaveAsync(result.RefreshToken, new RefreshTokenEntry
            {
                Subject = subject,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
            });

            return (result.AccessToken, result.RefreshToken);
        }

        [Fact]
        public async Task RefreshAsync_ShouldReturn_NewTokenPair()
        {
            var (accessToken, refreshToken) = await LoginAsync();

            var result = await _sut.RefreshAsync(accessToken, refreshToken);

            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBe(refreshToken);
        }

        [Fact]
        public async Task RefreshAsync_ShouldRevoke_OldRefreshToken()
        {
            var (accessToken, refreshToken) = await LoginAsync();

            await _sut.RefreshAsync(accessToken, refreshToken);

            var oldEntry = await _store.GetAsync(refreshToken);
            oldEntry!.IsRevoked.Should().BeTrue();
        }

        [Fact]
        public async Task RefreshAsync_ShouldPreserve_Claims()
        {
            var (accessToken, refreshToken) = await LoginAsync();

            var result = await _sut.RefreshAsync(accessToken, refreshToken);

            var principal = _jwtService.ValidateToken(result.AccessToken);
            principal!.FindFirst("role")?.Value.Should().Be("admin");
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_ForRevokedToken()
        {
            var (accessToken, refreshToken) = await LoginAsync();
            await _store.RevokeAsync(refreshToken);

            var act = async () => await _sut.RefreshAsync(accessToken, refreshToken);

            await act.Should().ThrowAsync<RefreshTokenException>()
                .WithMessage("*revocato*");
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_ForUnknownToken()
        {
            var (accessToken, _) = await LoginAsync();

            var act = async () => await _sut.RefreshAsync(accessToken, "fake-refresh-token");

            await act.Should().ThrowAsync<RefreshTokenException>();
        }

        [Fact]
        public async Task RevokeAllAsync_ShouldPrevent_FutureRefresh()
        {
            var (accessToken, refreshToken) = await LoginAsync("user-123");
            await _sut.RevokeAllAsync("user-123");

            var act = async () => await _sut.RefreshAsync(accessToken, refreshToken);

            await act.Should().ThrowAsync<RefreshTokenException>();
        }
    }
}
