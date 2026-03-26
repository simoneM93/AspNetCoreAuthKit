using AspNetCoreAuthKit.JWT.Models;

namespace AspNetCoreAuthKit.Tokens.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<TokenResult> RefreshAsync(string expiredAccessToken, string refreshToken, CancellationToken ct = default);
        Task RevokeAsync(string refreshToken, CancellationToken ct = default);
        Task RevokeAllAsync(string subject, CancellationToken ct = default);
    }
}
