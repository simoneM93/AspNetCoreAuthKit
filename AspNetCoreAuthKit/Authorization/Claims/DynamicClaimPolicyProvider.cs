using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AspNetCoreAuthKit.Authorization.Claims
{
    public class DynamicClaimPolicyProvider : IAuthorizationPolicyProvider
    {
        private const string PolicyPrefix = "__AuthKit_Claim__";
        private readonly DefaultAuthorizationPolicyProvider _fallback;

        public DynamicClaimPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallback = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => _fallback.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => _fallback.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (!policyName.StartsWith(PolicyPrefix))
                return _fallback.GetPolicyAsync(policyName);

            var parts = policyName[PolicyPrefix.Length..].Split("__", 2);
            if (parts.Length != 2)
                return _fallback.GetPolicyAsync(policyName);

            var claimType = parts[0];
            var allowedValues = parts[1].Split(",");

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(claimType, allowedValues)
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
    }
}
