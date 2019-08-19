using System;
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
            var mockUserInfos = CreateMockUsers().Select(u => UserUtility.CreateUserInfo(u)).ToList();
            UserUserInfo = mockUserInfos[0];
            AdminUserInfo = mockUserInfos[1];
            UserInfos = mockUserInfos;
        }

        public const string UserUsername = "user";
        public const string AdminUsername = "admin";
        public const string UserPassword = "user";
        public const string AdminPassword = "admin";

        // emmmmmmm. Never reuse the user instances because EF Core uses them which will cause strange things.
        internal static IEnumerable<User> CreateMockUsers()
        {
            var users = new List<User>();
            var passwordService = new PasswordService();
            users.Add(new User
            {
                Name = UserUsername,
                EncryptedPassword = passwordService.HashPassword(UserPassword),
                RoleString = UserUtility.IsAdminToRoleString(false),
                Avatar = UserAvatar.Create(DateTime.Now)
            });
            users.Add(new User
            {
                Name = AdminUsername,
                EncryptedPassword = passwordService.HashPassword(AdminPassword),
                RoleString = UserUtility.IsAdminToRoleString(true),
                Avatar = UserAvatar.Create(DateTime.Now)
            });
            return users;
        }

        public static IReadOnlyList<UserInfo> UserInfos { get; }

        public static UserInfo AdminUserInfo { get; }
        public static UserInfo UserUserInfo { get; }
    }
}
