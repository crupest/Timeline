using System;
using System.Collections.Generic;
using System.Linq;
using Timeline.Entities;
using Timeline.Services;

namespace Timeline.Models
{
    public static class UserConvert
    {
        public static UserInfo CreateUserInfo(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return new UserInfo(user.Name, UserRoleConvert.ToBool(user.RoleString));
        }

        internal static UserCache CreateUserCache(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return new UserCache
            {
                Username = user.Name,
                Administrator = UserRoleConvert.ToBool(user.RoleString),
                Version = user.Version
            };
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "No need.")]
    public static class UserRoleConvert
    {
        public const string UserRole = UserRoles.User;
        public const string AdminRole = UserRoles.Admin;

        public static string[] ToArray(bool administrator)
        {
            return administrator ? new string[] { UserRole, AdminRole } : new string[] { UserRole };
        }

        public static string[] ToArray(string s)
        {
            return s.Split(',').ToArray();
        }

        public static bool ToBool(IReadOnlyCollection<string> roles)
        {
            return roles.Contains(AdminRole);
        }

        public static string ToString(IReadOnlyCollection<string> roles)
        {
            return string.Join(',', roles);
        }

        public static string ToString(bool administrator)
        {
            return administrator ? UserRole + "," + AdminRole : UserRole;
        }

        public static bool ToBool(string s)
        {
            return s.Contains("admin", StringComparison.InvariantCulture);
        }
    }
}
