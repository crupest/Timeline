using System;
using System.Collections.Generic;
using System.Linq;
using TimelineApp.Entities;

namespace TimelineApp.Services
{
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
