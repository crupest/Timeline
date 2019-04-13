using System;
using System.Linq;
using Timeline.Models;

namespace Timeline.Entities
{
    public class UserInfo
    {
        public UserInfo()
        {

        }

        public UserInfo(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            Username = user.Name;
            Roles = user.RoleString.Split(',').Select(s => s.Trim()).ToArray();
        }

        public string Username { get; set; }
        public string[] Roles { get; set; }
    }
}
