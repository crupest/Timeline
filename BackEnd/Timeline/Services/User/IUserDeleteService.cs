using System;
using System.Threading.Tasks;

namespace Timeline.Services.User
{
    public interface IUserDeleteService
    {
        /// <summary>
        /// Delete a user of given username.
        /// </summary>
        /// <param name="username">Username of the user to delete. Can't be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="username"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is of bad format.</exception>
        /// <exception cref="EntityNotExistException">Thrown when the user does not exist. </exception>
        /// <exception cref="InvalidOperationOnRootUserException">Thrown when deleting root user.</exception>
        Task DeleteUserAsync(string username);
    }
}
