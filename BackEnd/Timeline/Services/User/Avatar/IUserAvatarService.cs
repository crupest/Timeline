using System;
using System.Threading.Tasks;
using Timeline.Helpers.Cache;
using Timeline.Models;
using Timeline.Services.Imaging;

namespace Timeline.Services.User.Avatar
{
    public interface IUserAvatarService
    {
        /// <summary>
        /// Get avatar digest of a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>The avatar digest.</returns>
        /// <exception cref="EntityNotExistException">Thrown when user does not exist.</exception>
        Task<ICacheableDataDigest> GetAvatarDigestAsync(long userId);

        /// <summary>
        /// Get avatar of a user. If the user has no avatar set, a default one is returned.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>The avatar.</returns>
        /// <exception cref="EntityNotExistException">Thrown when user does not exist.</exception>
        Task<ByteData> GetAvatarAsync(long userId);

        /// <summary>
        /// Set avatar for a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="avatar">The new avatar data.</param>
        /// <returns>The digest of the avatar.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="avatar"/> is null.</exception>
        /// <exception cref="EntityNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="ImageException">Thrown if avatar is of bad format.</exception>
        Task<ICacheableDataDigest> SetAvatarAsync(long userId, ByteData avatar);

        /// <summary>
        /// Remove avatar of a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <exception cref="EntityNotExistException">Thrown when user does not exist.</exception>
        Task DeleteAvatarAsync(long userId);
    }
}
