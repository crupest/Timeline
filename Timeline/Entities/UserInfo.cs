using System;
using System.Collections.Generic;
using System.Linq;
using Timeline.Models;

namespace Timeline.Entities
{
    public sealed class UserInfo
    {
        public UserInfo()
        {
        }

        public UserInfo(string username, params string[] roles)
        {
            Username = username;
            Roles = roles;
        }

        public static UserInfo Create(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Create(user.Name, user.RoleString);
        }

        public static UserInfo Create(string username, string roleString) => new UserInfo
        {
            Username = username,
            Roles = RolesFromString(roleString)
        };

        public string Username { get; set; }
        public string[] Roles { get; set; }

        public static IEqualityComparer<UserInfo> EqualityComparer { get; } = new EqualityComparerImpl();
        public static IComparer<UserInfo> Comparer { get; } = Comparer<UserInfo>.Create(Compare);

        private static string[] RolesFromString(string roleString)
        {
            if (roleString == null)
                return null;
            return roleString.Split(',').Select(r => r.Trim()).ToArray();
        }

        private class EqualityComparerImpl : IEqualityComparer<UserInfo>
        {
            bool IEqualityComparer<UserInfo>.Equals(UserInfo x, UserInfo y)
            {
                return Compare(x, y) == 0;
            }

            int IEqualityComparer<UserInfo>.GetHashCode(UserInfo obj)
            {
                return obj.Username.GetHashCode() ^ NormalizeRoles(obj.Roles).GetHashCode();
            }
        }

        private static string NormalizeRoles(string[] rawRoles)
        {
            var roles = rawRoles.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).ToList();
            roles.Sort();
            return string.Join(',', roles);
        }

        public static int Compare(UserInfo left, UserInfo right)
        {
            if (left == null)
            {
                if (right == null)
                    return 0;
                return -1;
            }

            if (right == null)
                return 1;

            var uc = string.Compare(left.Username, right.Username);
            if (uc != 0)
                return uc;

            var leftRoles = NormalizeRoles(left.Roles);
            var rightRoles = NormalizeRoles(right.Roles);

            return string.Compare(leftRoles, rightRoles);
        }

        public override string ToString()
        {
            return $"Username: {Username} ; Roles: {Roles}";
        }
    }
}
