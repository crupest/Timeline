using System.Threading.Tasks;
using Timeline.Helpers.Cache;
using Timeline.Models;

namespace Timeline.Services.User.Avatar
{
    /// <summary>
    /// Provider for default user avatar.
    /// </summary>
    /// <remarks>
    /// Mainly for unit tests.
    /// </remarks>
    public interface IDefaultUserAvatarProvider
    {
        /// <summary>
        /// Get the digest of default avatar.
        /// </summary>
        /// <returns>The digest.</returns>
        Task<ICacheableDataDigest> GetDefaultAvatarDigest();

        /// <summary>
        /// Get the default avatar.
        /// </summary>
        /// <returns>The avatar.</returns>
        Task<ByteData> GetDefaultAvatar();
    }
}
