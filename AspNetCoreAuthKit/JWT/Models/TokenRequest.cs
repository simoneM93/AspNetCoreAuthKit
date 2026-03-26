using System.Security.Claims;

namespace AspNetCoreAuthKit.JWT.Models
{
    public record TokenRequest
    {
        public required string Subject { get; init; }
        public IEnumerable<Claim> Claims { get; init; } = [];
        public TimeSpan? ExpiresIn { get; init; }
    }
}
