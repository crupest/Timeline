using System.Collections.Generic;
using System.Linq;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Services;

namespace Timeline.Tests.Helpers
{
    public static class TestMockUsers
    {
        static TestMockUsers()
        {
            var mockUsers = new List<User>();
            var passwordService = new PasswordService();

            mockUsers.Add(new User
            {
                Name = "user",
                EncryptedPassword = passwordService.HashPassword("user"),
                RoleString = UserUtility.IsAdminToRoleString(false),
                Version = 0,
            });
            mockUsers.Add(new User
            {
                Name = "admin",
                EncryptedPassword = passwordService.HashPassword("admin"),
                RoleString = UserUtility.IsAdminToRoleString(true),
                Version = 0,
            });

            MockUsers = mockUsers;

            var mockUserInfos = mockUsers.Select(u => UserUtility.CreateUserInfo(u)).ToList();
            mockUserInfos.Sort(UserInfoComparers.Comparer);
            MockUserInfos = mockUserInfos;
        }

        public static List<User> MockUsers { get; }

        public static IReadOnlyList<UserInfo> MockUserInfos { get; }
    }
}
