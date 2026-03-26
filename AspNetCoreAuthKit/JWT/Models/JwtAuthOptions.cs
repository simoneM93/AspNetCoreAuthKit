using Microsoft.IdentityModel.Tokens;

namespace AspNetCoreAuthKit.JWT.Models
{
    public class JwtAuthOptions
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public TimeSpan AccessTokenExpiry { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan RefreshTokenExpiry { get; set; } = TimeSpan.FromDays(7);
        public string Algorithm { get; set; } = SecurityAlgorithms.HmacSha256;
        public TimeSpan ClockSkew { get; set; } = TimeSpan.FromSeconds(30);
    }
}
