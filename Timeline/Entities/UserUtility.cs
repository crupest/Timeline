using System;
using System.Linq;
using Timeline.Models;
using Timeline.Services;

namespace Timeline.Entities
{
    public static class UserUtility
    {
        public const string UserRole = UserRoles.User;
        public const string AdminRole = UserRoles.Admin;

        public static string[] UserRoleArray { get; } = new string[] { UserRole };
        public static string[] AdminRoleArray { get; } = new string[] { UserRole, AdminRole };

        public static string[] IsAdminToRoleArray(bool isAdmin)
        {
            return isAdmin ? AdminRoleArray : UserRoleArray;
        }

        public static bool RoleArrayToIsAdmin(string[] roles)
        {
            return roles.Contains(AdminRole);
        }

        public static string[] RoleStringToRoleArray(string roleString)
        {
            return roleString.Split(',').ToArray();
        }

        public static string RoleArrayToRoleString(string[] roles)
        {
            return string.Join(',', roles);
        }

        public static string IsAdminToRoleString(bool isAdmin)
        {
            return RoleArrayToRoleString(IsAdminToRoleArray(isAdmin));
        }

        public static bool RoleStringToIsAdmin(string roleString)
        {
            return RoleArrayToIsAdmin(RoleStringToRoleArray(roleString));
        }

        public static UserInfo CreateUserInfo(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return new UserInfo(user.Name, RoleStringToIsAdmin(user.RoleString));
        }

        internal static UserCache CreateUserCache(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return new UserCache { Username = user.Name, Administrator = RoleStringToIsAdmin(user.RoleString), Version = user.Version };
        }
    }
}
