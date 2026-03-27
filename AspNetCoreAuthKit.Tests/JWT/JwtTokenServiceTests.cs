using AspNetCoreAuthKit.JWT;
using AspNetCoreAuthKit.JWT.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AspNetCoreAuthKit.Tests.JWT
{
    public class JwtTokenServiceTests
    {
        private readonly JwtTokenService _sut;
        private readonly JwtAuthOptions _options;

        public JwtTokenServiceTests()
        {
            _options = new JwtAuthOptions
            {
                SecretKey = "super-secret-key-long-enough-for-hmac-256!",
                Issuer = "test-issuer",
                Audience = "test-audience",
                AccessTokenExpiry = TimeSpan.FromMinutes(15),
                RefreshTokenExpiry = TimeSpan.FromDays(7),
                ClockSkew = TimeSpan.Zero
            };
            _sut = new JwtTokenService(Options.Create(_options));
        }

        [Fact]
        public void GenerateToken_ShouldReturn_ValidAccessToken()
        {
            var request = new TokenRequest { Subject = "user-123" };

            var result = _sut.GenerateToken(request);

            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
        }

        [Fact]
        public void GenerateToken_ShouldInclude_CustomClaims()
        {
            var request = new TokenRequest
            {
                Subject = "user-123",
                Claims = [new Claim("role", "admin")]
            };

            var result = _sut.GenerateToken(request);
            var principal = _sut.ValidateToken(result.AccessToken);

            principal.Should().NotBeNull();
            principal!.FindFirst("role")?.Value.Should().Be("admin");
        }

        [Fact]
        public void GenerateToken_ShouldRespect_CustomExpiry()
        {
            var customExpiry = TimeSpan.FromHours(2);
            var request = new TokenRequest
            {
                Subject = "user-123",
                ExpiresIn = customExpiry
            };

            var result = _sut.GenerateToken(request);

            result.ExpiresAt.Should().BeCloseTo(
                DateTimeOffset.UtcNow.Add(customExpiry),
                precision: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void ValidateToken_ShouldReturn_Principal_ForValidToken()
        {
            var request = new TokenRequest { Subject = "user-456" };
            var result = _sut.GenerateToken(request);

            var principal = _sut.ValidateToken(result.AccessToken);

            principal.Should().NotBeNull();
            principal!.FindFirst("sub")?.Value.Should().Be("user-456");
        }

        [Fact]
        public void ValidateToken_ShouldReturn_Null_ForInvalidToken()
        {
            var principal = _sut.ValidateToken("this.is.not.a.valid.token");

            principal.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_ShouldReturn_Null_ForTamperedToken()
        {
            var request = new TokenRequest { Subject = "user-123" };
            var result = _sut.GenerateToken(request);

            var tampered = result.AccessToken[..^5] + "XXXXX";
            var principal = _sut.ValidateToken(tampered);

            principal.Should().BeNull();
        }

        [Fact]
        public void ValidateTokenIgnoreExpiry_ShouldReturn_Principal_ForExpiredToken()
        {
            var result = _sut.GenerateToken(new TokenRequest
            {
                Subject = "user-1",
                Claims = [new Claim("role", "admin")],
                ExpiresIn = TimeSpan.FromSeconds(1)
            });

            Thread.Sleep(2000);

            var failedPrincipal = _sut.ValidateToken(result.AccessToken);
            Assert.Null(failedPrincipal);

            var principal = _sut.ValidateTokenIgnoreExpiry(result.AccessToken);
            Assert.NotNull(principal);
            Assert.Equal("user-1", principal.FindFirst("sub")?.Value);
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturn_UniqueTokens()
        {
            var token1 = _sut.GenerateRefreshToken();
            var token2 = _sut.GenerateRefreshToken();

            token1.Should().NotBe(token2);
            token1.Length.Should().BeGreaterThan(32);
        }
    }
}
