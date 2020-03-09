using Microsoft.AspNetCore.Authorization;
using TimelineApp.Entities;

namespace TimelineApp.Auth
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
