using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services
{
    public class Avatar
    {
        public string Type { get; set; }
        public byte[] Data { get; set; }
    }

    /// <summary>
    /// Thrown when avatar is of bad format.
    /// </summary>
    [Serializable]
    public class AvatarDataException : Exception
    {
        public AvatarDataException(Avatar avatar, string message) : base(message) { Avatar = avatar; }
        public AvatarDataException(Avatar avatar, string message, Exception inner) : base(message, inner) { Avatar = avatar; }
        protected AvatarDataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public Avatar Avatar { get; set; }
    }

    /// <summary>
    /// Provider for default user avatar.
    /// </summary>
    /// <remarks>
    /// Mainly for unit tests.
    /// </remarks>
    public interface IDefaultUserAvatarProvider
    {
        /// <summary>
        /// Get the default avatar.
        /// </summary>
        Task<Avatar> GetDefaultAvatar();
    }

    public interface IUserAvatarService
    {
        /// <summary>
        /// Get avatar of a user. If the user has no avatar, a default one is returned.
        /// </summary>
        /// <param name="username">The username of the user to get avatar of.</param>
        /// <returns>The avatar.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="username"/> is null or empty.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user does not exist.</exception>
        Task<Avatar> GetAvatar(string username);

        /// <summary>
        /// Set avatar for a user.
        /// </summary>
        /// <param name="username">The username of the user to set avatar for.</param>
        /// <param name="avatar">The avatar. Can be null to delete the saved avatar.</param>
        /// <exception cref="ArgumentException">Throw if <paramref name="username"/> is null or empty.
        /// Or thrown if <paramref name="avatar"/> is not null but <see cref="Avatar.Type"/> is null or empty or <see cref="Avatar.Data"/> is null.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user does not exist.</exception>
        /// <exception cref="AvatarDataException">Thrown if avatar is of bad format.</exception>
        Task SetAvatar(string username, Avatar avatar);
    }

    public class DefaultUserAvatarProvider : IDefaultUserAvatarProvider
    {
        private readonly IHostingEnvironment _environment;

        public DefaultUserAvatarProvider(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<Avatar> GetDefaultAvatar()
        {
            return new Avatar
            {
                Type = "image/png",
                Data = await File.ReadAllBytesAsync(Path.Combine(_environment.ContentRootPath, "default-avatar.png"))
            };
        }
    }

    public class UserAvatarService : IUserAvatarService
    {

        private readonly ILogger<UserAvatarService> _logger;

        private readonly DatabaseContext _database;

        private readonly IDefaultUserAvatarProvider _defaultUserAvatarProvider;

        public UserAvatarService(ILogger<UserAvatarService> logger, DatabaseContext database, IDefaultUserAvatarProvider defaultUserAvatarProvider)
        {
            _logger = logger;
            _database = database;
            _defaultUserAvatarProvider = defaultUserAvatarProvider;
        }

        public async Task<Avatar> GetAvatar(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username is null or empty.", nameof(username));

            var user = await _database.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(username);

            await _database.Entry(user).Reference(u => u.Avatar).LoadAsync();
            var avatar = user.Avatar;

            if (avatar == null)
            {
                return await _defaultUserAvatarProvider.GetDefaultAvatar();
            }
            else
            {
                return new Avatar
                {
                    Type = avatar.Type,
                    Data = avatar.Data
                };
            }
        }

        public async Task SetAvatar(string username, Avatar avatar)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username is null or empty.", nameof(username));

            if (avatar != null)
            {
                if (string.IsNullOrEmpty(avatar.Type))
                    throw new ArgumentException("Type of avatar is null or empty.", nameof(avatar));
                if (avatar.Data == null)
                    throw new ArgumentException("Data of avatar is null.", nameof(avatar));
            }

            var user = await _database.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
            if (user == null)
                throw new UserNotExistException(username);

            await _database.Entry(user).Reference(u => u.Avatar).LoadAsync();
            var avatarEntity = user.Avatar;

            if (avatar == null)
            {
                if (avatarEntity == null)
                    return;
                else
                {
                    _database.UserAvatars.Remove(avatarEntity);
                    await _database.SaveChangesAsync();
                }
            }
            else
            {
                // TODO: Use image library to check the format to prohibit bad data.
                if (avatarEntity == null)
                {
                    user.Avatar = new UserAvatar
                    {
                        Type = avatar.Type,
                        Data = avatar.Data
                    };
                }
                else
                {
                    avatarEntity.Type = avatar.Type;
                    avatarEntity.Data = avatar.Data;
                }
                await _database.SaveChangesAsync();
            }
        }
    }

    public static class UserAvatarServiceCollectionExtensions
    {
        public static void AddUserAvatarService(this IServiceCollection services)
        {
            services.AddScoped<IUserAvatarService, UserAvatarService>();
            services.AddSingleton<IDefaultUserAvatarProvider, DefaultUserAvatarProvider>();
        }
    }
}
