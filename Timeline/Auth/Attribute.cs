using Microsoft.AspNetCore.Authorization;
using Timeline.Entities;

namespace Timeline.Auth
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
