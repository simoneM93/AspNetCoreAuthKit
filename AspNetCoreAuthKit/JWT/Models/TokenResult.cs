namespace AspNetCoreAuthKit.JWT.Models
{
    public record TokenResult
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
        public required DateTimeOffset ExpiresAt { get; init; }
    }
}
