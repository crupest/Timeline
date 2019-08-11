using System.Collections.Generic;
using System.Linq;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Services;

namespace Timeline.Tests.Mock.Data
{
    public static class MockUsers
    {
        static MockUsers()
        {
            var mockUsers = new List<User>();
            var passwordService = new PasswordService();

            mockUsers.Add(new User
            {
                Name = UserUsername,
                EncryptedPassword = passwordService.HashPassword(UserPassword),
                RoleString = UserUtility.IsAdminToRoleString(false),
                Version = 0,
            });
            mockUsers.Add(new User
            {
                Name = AdminUsername,
                EncryptedPassword = passwordService.HashPassword(AdminPassword),
                RoleString = UserUtility.IsAdminToRoleString(true),
                Version = 0,
            });

            Users = mockUsers;

            var mockUserInfos = mockUsers.Select(u => UserUtility.CreateUserInfo(u)).ToList();
            UserUserInfo = mockUserInfos[0];
            AdminUserInfo = mockUserInfos[1];
            UserInfos = mockUserInfos;
        }

        public const string UserUsername = "user";
        public const string AdminUsername = "admin";
        public const string UserPassword = "user";
        public const string AdminPassword = "admin";

        internal static IReadOnlyList<User> Users { get; }
        public static IReadOnlyList<UserInfo> UserInfos { get; }

        public static UserInfo AdminUserInfo { get; }
        public static UserInfo UserUserInfo { get; }
    }
}
