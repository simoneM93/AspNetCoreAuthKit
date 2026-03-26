namespace AspNetCoreAuthKit.Tokens.Models
{
    public record RefreshTokenEntry
    {
        public required string Subject { get; init; }
        public required DateTimeOffset ExpiresAt { get; init; }
        public bool IsRevoked { get; init; } = false;
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

        public Dictionary<string, string> Metadata { get; init; } = [];

        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
        public bool IsValid => !IsRevoked && !IsExpired;
    }
}
