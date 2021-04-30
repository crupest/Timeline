using System;
using System.Threading.Tasks;

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
        Task<bool> CheckUserExistenceAsync(long id);

        /// <summary>
        /// Get the user id of given username.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>The id of the user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="EntityNotExistException">Thrown when the user with given username does not exist.</exception>
        Task<long> GetUserIdByUsernameAsync(string username);

        /// <summary>
        /// Get the username modified time of a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>The time.</returns>
        /// <exception cref="EntityNotExistException">Thrown when user does not exist.</exception>
        Task<DateTime> GetUsernameLastModifiedTimeAsync(long userId);
    }
}
