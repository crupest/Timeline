using System;
using System.Collections.Generic;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Services;

namespace Timeline.Tests.Mock.Data
{
    public class MockUser
    {
        public MockUser(string username, string password, bool administrator)
        {
            Info = new UserInfo(username, administrator);
            Password = password;
        }

        public UserInfo Info { get; set; }
        public string Username => Info.Username;
        public string Password { get; set; }
        public bool Administrator => Info.Administrator;


        public static MockUser User { get; } = new MockUser("user", "userpassword", false);
        public static MockUser Admin { get; } = new MockUser("admin", "adminpassword", true);

        public static IReadOnlyList<UserInfo> UserInfoList { get; } = new List<UserInfo> { User.Info, Admin.Info };

        // emmmmmmm. Never reuse the user instances because EF Core uses them, which will cause strange things.
        public static IEnumerable<User> CreateMockEntities()
        {
            var passwordService = new PasswordService();
            User Create(MockUser user)
            {
                return new User
                {
                    Name = user.Username,
                    EncryptedPassword = passwordService.HashPassword(user.Password),
                    RoleString = UserRoleConvert.ToString(user.Administrator),
                    Avatar = UserAvatar.Create(DateTime.Now)
                };
            }

            return new List<User>
            {
                Create(User),
                Create(Admin)
            };
        }
    }
}
