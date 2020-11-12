using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Timeline.Auth
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public const string PolicyPrefix = "Permission-";

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return Task.FromResult(new AuthorizationPolicyBuilder(AuthenticationConstants.Scheme).RequireAuthenticatedUser().Build());
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return Task.FromResult<AuthorizationPolicy?>(null);
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var permissions = policyName[PolicyPrefix.Length..].Split(',');

                var policy = new AuthorizationPolicyBuilder(AuthenticationConstants.Scheme);
                policy.AddRequirements(new ClaimsAuthorizationRequirement(AuthenticationConstants.PermissionClaimName, permissions));
                return Task.FromResult<AuthorizationPolicy?>(policy.Build());
            }
            return Task.FromResult<AuthorizationPolicy?>(null);
        }
    }
}
