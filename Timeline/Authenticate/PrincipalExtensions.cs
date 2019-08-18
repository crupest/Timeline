using System.Security.Principal;
using Timeline.Entities;

namespace Timeline.Authenticate
{
    public static class PrincipalExtensions
    {
        public static bool IsAdmin(this IPrincipal principal)
        {
            return principal.IsInRole(UserRoles.Admin);
        }
    }
}
