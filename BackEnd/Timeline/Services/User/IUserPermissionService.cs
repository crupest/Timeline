using System.Threading.Tasks;

namespace Timeline.Services.User
{
    public interface IUserPermissionService
    {
        /// <summary>
        /// Get permissions of a user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="checkUserExistence">Whether check the user's existence.</param>
        /// <returns>The permission list.</returns>
        /// <exception cref="EntityNotExistException">Thrown when <paramref name="checkUserExistence"/> is true and user does not exist.</exception>
        Task<UserPermissions> GetPermissionsOfUserAsync(long userId, bool checkUserExistence = true);

        /// <summary>
        /// Add a permission to user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="permission">The new permission.</param>
        /// <exception cref="EntityNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="InvalidOperationOnRootUserException">Thrown when change root user's permission.</exception>
        Task AddPermissionToUserAsync(long userId, UserPermission permission);

        /// <summary>
        /// Remove a permission from user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="permission">The permission.</param>
        /// <param name="checkUserExistence">Whether check the user's existence.</param>
        /// <exception cref="EntityNotExistException">Thrown when <paramref name="checkUserExistence"/> is true and user does not exist.</exception>
        /// <exception cref="InvalidOperationOnRootUserException">Thrown when change root user's permission.</exception>
        Task RemovePermissionFromUserAsync(long userId, UserPermission permission, bool checkUserExistence = true);
    }
}
