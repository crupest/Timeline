using System.Security.Principal;
using TimelineApp.Entities;

namespace TimelineApp.Auth
{
    internal static class PrincipalExtensions
    {
        internal static bool IsAdministrator(this IPrincipal principal)
        {
            return principal.IsInRole(UserRoles.Admin);
        }
    }
}
