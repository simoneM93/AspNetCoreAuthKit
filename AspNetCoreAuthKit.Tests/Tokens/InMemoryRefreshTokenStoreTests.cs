using AspNetCoreAuthKit.Tokens;
using AspNetCoreAuthKit.Tokens.Models;
using FluentAssertions;

namespace AspNetCoreAuthKit.Tests.Tokens
{
    public class InMemoryRefreshTokenStoreTests
    {
        private readonly InMemoryRefreshTokenStore _sut = new();

        private static RefreshTokenEntry ValidEntry(string subject = "user-123") => new()
        {
            Subject = subject,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        [Fact]
        public async Task SaveAndGet_ShouldReturn_StoredEntry()
        {
            var entry = ValidEntry();
            await _sut.SaveAsync("token-1", entry);

            var result = await _sut.GetAsync("token-1");

            result.Should().NotBeNull();
            result!.Subject.Should().Be("user-123");
        }

        [Fact]
        public async Task Get_ShouldReturn_Null_ForUnknownToken()
        {
            var result = await _sut.GetAsync("non-existent-token");

            result.Should().BeNull();
        }

        [Fact]
        public async Task Get_ShouldReturn_Null_ForExpiredToken()
        {
            var expiredEntry = new RefreshTokenEntry
            {
                Subject = "user-123",
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1)
            };
            await _sut.SaveAsync("expired-token", expiredEntry);

            var result = await _sut.GetAsync("expired-token");

            result.Should().BeNull();
        }

        [Fact]
        public async Task Revoke_ShouldInvalidate_Token()
        {
            await _sut.SaveAsync("token-1", ValidEntry());
            await _sut.RevokeAsync("token-1");

            var result = await _sut.GetAsync("token-1");

            result!.IsRevoked.Should().BeTrue();
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task RevokeAll_ShouldInvalidate_AllTokensOfSubject()
        {
            await _sut.SaveAsync("token-1", ValidEntry("user-123"));
            await _sut.SaveAsync("token-2", ValidEntry("user-123"));
            await _sut.SaveAsync("token-3", ValidEntry("user-999"));

            await _sut.RevokeAllAsync("user-123");

            var t1 = await _sut.GetAsync("token-1");
            var t2 = await _sut.GetAsync("token-2");
            var t3 = await _sut.GetAsync("token-3");

            t1!.IsRevoked.Should().BeTrue();
            t2!.IsRevoked.Should().BeTrue();
            t3!.IsRevoked.Should().BeFalse();
        }
    }
}
