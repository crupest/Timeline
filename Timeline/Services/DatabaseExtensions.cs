using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models.Validation;

namespace Timeline.Services
{
    internal static class DatabaseExtensions
    {
        private static readonly UsernameValidator usernameValidator = new UsernameValidator();

        /// <summary>
        /// Check the existence and get the id of the user.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <returns>The user id.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="username"/> is null.</exception>
        /// <exception cref="UsernameBadFormatException">Thrown if <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown if user does not exist.</exception>
        internal static async Task<long> CheckAndGetUser(DbSet<UserEntity> userDbSet, string? username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            var (result, message) = usernameValidator.Validate(username);
            if (!result)
                throw new UsernameBadFormatException(username, message);

            var userId = await userDbSet.Where(u => u.Name == username).Select(u => u.Id).SingleOrDefaultAsync();
            if (userId == 0)
                throw new UserNotExistException(username);
            return userId;
        }
    }
}
