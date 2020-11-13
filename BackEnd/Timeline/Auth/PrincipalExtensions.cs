using System;
using System.Security.Claims;
using Timeline.Services;

namespace Timeline.Auth
{
    internal static class PrincipalExtensions
    {
        internal static bool HasPermission(this ClaimsPrincipal principal, UserPermission permission)
        {
            return principal.HasClaim(
                claim => claim.Type == AuthenticationConstants.PermissionClaimName && string.Equals(claim.Value, permission.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
