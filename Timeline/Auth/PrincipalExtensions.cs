using System.Security.Principal;
using Timeline.Entities;

namespace Timeline.Auth
{
    internal static class PrincipalExtensions
    {
        internal static bool IsAdministrator(this IPrincipal principal)
        {
            return principal.IsInRole(UserRoles.Admin);
        }
    }
}
