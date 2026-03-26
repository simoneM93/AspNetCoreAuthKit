using AspNetCoreAuthKit.Tokens.Models;

namespace AspNetCoreAuthKit.Tokens.Interfaces
{
    public interface IRefreshTokenStore
    {
        Task SaveAsync(string token, RefreshTokenEntry entry, CancellationToken ct = default);

        Task<RefreshTokenEntry?> GetAsync(string token, CancellationToken ct = default);

        Task RevokeAsync(string token, CancellationToken ct = default);

        Task RevokeAllAsync(string subject, CancellationToken ct = default);
    }
}
