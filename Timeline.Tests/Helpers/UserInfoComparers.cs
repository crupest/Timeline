using System;
using System.Collections.Generic;
using System.Linq;
using Timeline.Entities;

namespace Timeline.Tests.Helpers
{
    public static class UserInfoComparers
    {
        public static IEqualityComparer<UserInfo> EqualityComparer { get; } = new EqualityComparerImpl();
        public static IComparer<UserInfo> Comparer { get; } = Comparer<UserInfo>.Create(Compare);


        private class EqualityComparerImpl : IEqualityComparer<UserInfo>
        {
            bool IEqualityComparer<UserInfo>.Equals(UserInfo x, UserInfo y)
            {
                return Compare(x, y) == 0;
            }

            int IEqualityComparer<UserInfo>.GetHashCode(UserInfo obj)
            {
                return obj.Username.GetHashCode() ^ obj.IsAdmin.GetHashCode();
            }
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

            if (left.IsAdmin == right.IsAdmin)
                return 0;

            return left.IsAdmin ? -1 : 1;
        }
    }
}
