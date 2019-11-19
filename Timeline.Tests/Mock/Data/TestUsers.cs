using System.Collections.Generic;
using Timeline.Models;

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
    }
}
