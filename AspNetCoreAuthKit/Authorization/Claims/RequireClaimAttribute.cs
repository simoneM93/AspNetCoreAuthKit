using Microsoft.AspNetCore.Authorization;

namespace AspNetCoreAuthKit.Authorization.Claims
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireClaimAttribute : AuthorizeAttribute
    {
        public RequireClaimAttribute(string claimType, params string[] allowedValues)
        {
            Policy = BuildPolicyName(claimType, allowedValues);
        }

        public static string BuildPolicyName(string claimType, string[] allowedValues)
        {
            var values = string.Join(",", allowedValues.OrderBy(v => v));
            return $"__AuthKit_Claim__{claimType}__{values}";
        }
    }
}
