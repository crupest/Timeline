using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models.Validation;

namespace Timeline.Services.User
{
    /// <summary>
    /// This service provide some basic user features, which should be used internally for other services.
    /// </summary>
    public interface IBasicUserService
    {
        /// <summary>
        /// Check if a user exists.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        /// <returns>True if exists. Otherwise false.</returns>
        Task<bool> CheckUserExistence(long id);

        /// <summary>
        /// Get the user id of given username.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>The id of the user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user with given username does not exist.</exception>
        Task<long> GetUserIdByUsername(string username);

        /// <summary>
        /// Get the username modified time of a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>The time.</returns>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        Task<DateTime> GetUsernameLastModifiedTime(long userId);
    }

    public class BasicUserService : IBasicUserService
    {
        private readonly DatabaseContext _database;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();

        public BasicUserService(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<bool> CheckUserExistence(long id)
        {
            return await _database.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<long> GetUserIdByUsername(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            if (!_usernameValidator.Validate(username, out var message))
                throw new ArgumentException(message);

            var entity = await _database.Users.Where(user => user.Username == username).Select(u => new { u.Id }).SingleOrDefaultAsync();

            if (entity == null)
                throw new UserNotExistException(username);

            return entity.Id;
        }

        public async Task<DateTime> GetUsernameLastModifiedTime(long userId)
        {
            var entity = await _database.Users.Where(u => u.Id == userId).Select(u => new { u.UsernameChangeTime }).SingleOrDefaultAsync();

            if (entity is null)
                throw new UserNotExistException(userId);

            return entity.UsernameChangeTime;
        }
    }

    public static class BasicUserServiceExtensions
    {
        public static async Task ThrowIfUserNotExist(this IBasicUserService service, long userId)
        {
            if (!await service.CheckUserExistence(userId))
            {
                throw new UserNotExistException(userId);
            }
        }
    }
}
