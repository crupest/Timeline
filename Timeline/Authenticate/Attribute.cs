using Microsoft.AspNetCore.Authorization;
using Timeline.Models;

namespace Timeline.Authenticate
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public AdminAuthorizeAttribute()
        {
            Roles = UserRoles.Admin;
        }
    }

    public class UserAuthorizeAttribute : AuthorizeAttribute
    {
        public UserAuthorizeAttribute()
        {
            Roles = UserRoles.User;
        }
    }
}
