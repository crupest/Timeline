using System;
using System.Security.Claims;
using Timeline.Services.User;

namespace Timeline.Auth
{
    public static class PrincipalExtensions
    {
        public static long? GetUserId(this ClaimsPrincipal? principal)
        {
            if (principal is null) return null;

            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return null;

            if (long.TryParse(claim.Value, out var value))
                return value;

            throw new InvalidOperationException(Resource.ExceptionUserIdentifierClaimBadFormat);
        }

        public static bool HasPermission(this ClaimsPrincipal? principal, UserPermission permission)
        {
            if (principal is null) return false;
            return principal.HasClaim(
                claim => claim.Type == AuthenticationConstants.PermissionClaimName && string.Equals(claim.Value, permission.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
