using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Try to anthenticate with the given username and password.
        /// </summary>
        /// <param name="username">The username of the user to be anthenticated.</param>
        /// <param name="password">The password of the user to be anthenticated.</param>
        /// <returns><c>null</c> if anthentication failed.
        /// An instance of <see cref="User"/> if anthentication succeeded.</returns>
        User Authenticate(string username, string password);
    }

    public class UserService : IUserService
    {
        private readonly IList<User> _users = new List<User>{
            new User { Id = 0, Username = "admin", Password = "admin", Roles = new string[] { "User", "Admin" } },
            new User { Id = 1, Username = "user", Password = "user", Roles = new string[] { "User"} }
        };

        public User Authenticate(string username, string password)
        {
            return _users.FirstOrDefault(user => user.Username == username && user.Password == password);
        }
    }
}
