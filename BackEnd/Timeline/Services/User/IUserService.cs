using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.User
{
    public interface IUserService : IBasicUserService
    {
        /// <summary>
        /// Try to get a user by id.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        /// <returns>The user info.</returns>
        /// <exception cref="EntityNotExistException">Thrown when the user with given id does not exist.</exception>
        Task<UserEntity> GetUserAsync(long id);

        /// <summary>
        /// List all users.
        /// </summary>
        /// <returns>The user info of users.</returns>
        Task<List<UserEntity>> GetUsersAsync();

        /// <summary>
        /// Create a user with given info.
        /// </summary>
        /// <param name="param">Info of new user.</param>
        /// <returns>The the new user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="param"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when param field is illegal.</exception>
        /// <exception cref="EntityAlreadyExistException">Thrown when a user with given username already exists.</exception>
        Task<UserEntity> CreateUserAsync(CreateUserParams param);

        /// <summary>
        /// Modify a user.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        /// <param name="param">The new information.</param>
        /// <returns>The new user info.</returns>
        /// <exception cref="ArgumentException">Thrown when some fields in <paramref name="param"/> is bad.</exception>
        /// <exception cref="EntityNotExistException">Thrown when user with given id does not exist.</exception>
        /// <remarks>
        /// Version will increase if password is changed.
        /// </remarks>
        Task<UserEntity> ModifyUserAsync(long id, ModifyUserParams? param);

        /// <summary>
        /// Try to verify the given username and password.
        /// </summary>
        /// <param name="username">The username of the user to verify.</param>
        /// <param name="password">The password of the user to verify.</param>
        /// <returns>User id.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format or <paramref name="password"/> is empty.</exception>
        /// <exception cref="EntityNotExistException">Thrown when the user with given username does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown when password is wrong.</exception>
        Task<long> VerifyCredential(string username, string password);

        /// <summary>
        /// Try to change a user's password with old password.
        /// </summary>
        /// <param name="id">The id of user to change password of.</param>
        /// <param name="oldPassword">Old password.</param>
        /// <param name="newPassword">New password.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="oldPassword"/> or <paramref name="newPassword"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="oldPassword"/> or <paramref name="newPassword"/> is empty.</exception>
        /// <exception cref="EntityNotExistException">Thrown if the user with given username does not exist.</exception>
        /// <exception cref="BadPasswordException">Thrown if the old password is wrong.</exception>
        Task ChangePassword(long id, string oldPassword, string newPassword);
    }
}
