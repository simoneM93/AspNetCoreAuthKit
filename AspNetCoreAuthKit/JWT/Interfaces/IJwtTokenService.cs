using AspNetCoreAuthKit.JWT.Models;
using System.Security.Claims;

namespace AspNetCoreAuthKit.JWT.Interfaces
{
    public interface IJwtTokenService
    {
        TokenResult GenerateToken(TokenRequest request);
        ClaimsPrincipal? ValidateToken(string token);
        string GenerateRefreshToken();

        ClaimsPrincipal? ValidateTokenIgnoreExpiry(string token);
    }
}
