using System.Collections.Generic;
using Timeline.Models.Http;

namespace Timeline.Tests.Helpers
{
    public class MockUser
    {
        public MockUser(string username, string password, bool administrator)
        {
            Info = new User { Username = username, Administrator = administrator };
            Password = password;
        }

        public User Info { get; set; }
        public string Username => Info.Username;
        public string Password { get; set; }
        public bool Administrator => Info.Administrator;

        public static MockUser User { get; } = new MockUser("user", "userpassword", false);
        public static MockUser Admin { get; } = new MockUser("admin", "adminpassword", true);

        public static IReadOnlyList<User> UserInfoList { get; } = new List<User> { User.Info, Admin.Info };
    }
}
