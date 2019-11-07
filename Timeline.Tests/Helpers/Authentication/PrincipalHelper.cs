using System.Linq;
using System.Security.Claims;
using Timeline.Models;

namespace Timeline.Tests.Helpers.Authentication
{
    public static class PrincipalHelper
    {
        internal const string AuthScheme = "TESTAUTH";

        internal static ClaimsPrincipal Create(string username, bool administrator)
        {
            var identity = new ClaimsIdentity(AuthScheme);
            identity.AddClaim(new Claim(identity.NameClaimType, username, ClaimValueTypes.String));
            identity.AddClaims(UserRoleConvert.ToArray(administrator).Select(role => new Claim(identity.RoleClaimType, role, ClaimValueTypes.String)));

            var principal = new ClaimsPrincipal();
            principal.AddIdentity(identity);

            return principal;
        }
    }
}
