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
            var passwordService = new PasswordService(null);

            mockUsers.Add(new User
            {
                Name = "user",
                EncryptedPassword = passwordService.HashPassword("user"),
                RoleString = "user"
            });
            mockUsers.Add(new User
            {
                Name = "admin",
                EncryptedPassword = passwordService.HashPassword("admin"),
                RoleString = "user,admin"
            });

            MockUsers = mockUsers;

            var mockUserInfos = mockUsers.Select(u => UserInfo.Create(u)).ToList();
            mockUserInfos.Sort(UserInfo.Comparer);
            MockUserInfos = mockUserInfos;
        }

        public static List<User> MockUsers { get; }

        public static IReadOnlyList<UserInfo> MockUserInfos { get; }
    }
}
