using AspNetCoreAuthKit.Tokens.Interfaces;
using AspNetCoreAuthKit.Tokens.Models;
using System.Collections.Concurrent;

namespace AspNetCoreAuthKit.Tokens
{
    public class InMemoryRefreshTokenStore : IRefreshTokenStore
    {
        private readonly ConcurrentDictionary<string, RefreshTokenEntry> _store = new();

        public Task SaveAsync(string token, RefreshTokenEntry entry, CancellationToken ct = default)
        {
            _store[token] = entry;
            return Task.CompletedTask;
        }

        public Task<RefreshTokenEntry?> GetAsync(string token, CancellationToken ct = default)
        {
            _store.TryGetValue(token, out var entry);

            if (entry is not null && entry.IsExpired)
            {
                _store.TryRemove(token, out _);
                return Task.FromResult<RefreshTokenEntry?>(null);
            }

            return Task.FromResult(entry);
        }

        public Task RevokeAsync(string token, CancellationToken ct = default)
        {
            if (_store.TryGetValue(token, out var entry))
                _store[token] = entry with { IsRevoked = true };

            return Task.CompletedTask;
        }

        public Task RevokeAllAsync(string subject, CancellationToken ct = default)
        {
            foreach (var (key, entry) in _store)
            {
                if (entry.Subject == subject)
                    _store[key] = entry with { IsRevoked = true };
            }

            return Task.CompletedTask;
        }
    }
}
