using AspNetCoreAuthKit.Authorization.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AspNetCoreAuthKit.Tests.Authorization
{
    public class DynamicClaimPolicyProviderTests
    {
        private readonly DynamicClaimPolicyProvider _sut;

        public DynamicClaimPolicyProviderTests()
        {
            var options = Options.Create(new AuthorizationOptions());
            _sut = new DynamicClaimPolicyProvider(options);
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldBuild_PolicyForClaimPrefix()
        {
            var policyName = RequireClaimAttribute.BuildPolicyName("role", ["admin"]);

            var policy = await _sut.GetPolicyAsync(policyName);

            policy.Should().NotBeNull();
            policy!.Requirements.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldReturn_Null_ForUnknownPolicy()
        {
            var policy = await _sut.GetPolicyAsync("non-existent-policy");

            policy.Should().BeNull();
        }

        [Fact]
        public void BuildPolicyName_ShouldBe_Deterministic()
        {
            var name1 = RequireClaimAttribute.BuildPolicyName("role", ["admin", "moderator"]);
            var name2 = RequireClaimAttribute.BuildPolicyName("role", ["moderator", "admin"]);

            name1.Should().Be(name2);
        }
    }
}
