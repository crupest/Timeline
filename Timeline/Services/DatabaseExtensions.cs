using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services
{
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Check the existence and get the id of the user.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <returns>The user id.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="username"/> is null or empty.</exception>
        /// <exception cref="UserNotExistException">Thrown if user does not exist.</exception>
        public static async Task<long> CheckAndGetUser(DbSet<User> userDbSet, string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username is null or empty.", nameof(username));

            var userId = await userDbSet.Where(u => u.Name == username).Select(u => u.Id).SingleOrDefaultAsync();
            if (userId == 0)
                throw new UserNotExistException(username);
            return userId;
        }
    }
}
