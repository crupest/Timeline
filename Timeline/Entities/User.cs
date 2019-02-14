using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timeline.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string[] Roles { get; set; }

        public UserInfo GetUserInfo()
        {
            return new UserInfo
            {
                Username = this.Username,
                Roles = this.Roles
            };
        }
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string[] Roles { get; set; }
    }
}
