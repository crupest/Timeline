using System.Security.Principal;
using Timeline.Entities;

namespace Timeline.Authentication
{
    internal static class PrincipalExtensions
    {
        internal static bool IsAdministrator(this IPrincipal principal)
        {
            return principal.IsInRole(UserRoles.Admin);
        }
    }
}
