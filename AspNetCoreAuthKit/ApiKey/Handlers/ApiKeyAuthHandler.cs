using AspNetCoreAuthKit.ApiKey.Constants;
using AspNetCoreAuthKit.ApiKey.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AspNetCoreAuthKit.ApiKey.Handlers
{
    public class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthSchemeOptions>
    {
        private readonly ApiKeyOptions _apiKeyOptions;

        public ApiKeyAuthHandler(
            IOptionsMonitor<ApiKeyAuthSchemeOptions> schemeOptions,
            IOptions<ApiKeyOptions> apiKeyOptions,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(schemeOptions, logger, encoder)
        {
            _apiKeyOptions = apiKeyOptions.Value;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string? apiKey = ExtractApiKey();

            if (string.IsNullOrWhiteSpace(apiKey))
                return AuthenticateResult.NoResult();

            bool isValid = await IsValidKeyAsync(apiKey);

            if (!isValid)
                return AuthenticateResult.Fail("API Key non valida.");

            var claims = new[]
            {
            new Claim(ApiKeyConstants.ClaimType, apiKey),
            new Claim(ClaimTypes.AuthenticationMethod, ApiKeyConstants.AuthenticationScheme)
        };

            var identity = new ClaimsIdentity(claims, ApiKeyConstants.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, ApiKeyConstants.AuthenticationScheme);

            return AuthenticateResult.Success(ticket);
        }

        private string? ExtractApiKey()
        {
            if (Request.Headers.TryGetValue(_apiKeyOptions.HeaderName, out var headerValue))
                return headerValue.ToString();

            if (_apiKeyOptions.QueryParamName is not null &&
                Request.Query.TryGetValue(_apiKeyOptions.QueryParamName, out var queryValue))
                return queryValue.ToString();

            return null;
        }

        private async Task<bool> IsValidKeyAsync(string apiKey)
        {
            if (_apiKeyOptions.ValidateKeyAsync is not null)
                return await _apiKeyOptions.ValidateKeyAsync(apiKey);

            return _apiKeyOptions.ValidKeys.Contains(apiKey);
        }
    }

    public class ApiKeyAuthSchemeOptions : AuthenticationSchemeOptions { }
}
