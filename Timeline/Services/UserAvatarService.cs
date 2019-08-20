using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
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

    public class AvatarInfo
    {
        public Avatar Avatar { get; set; }
        public DateTime LastModified { get; set; }
    }

    /// <summary>
    /// Thrown when avatar is of bad format.
    /// </summary>
    [Serializable]
    public class AvatarDataException : Exception
    {
        public enum ErrorReason
        {
            /// <summary>
            /// Decoding image failed.
            /// </summary>
            CantDecode,
            /// <summary>
            /// Decoding succeeded but the real type is not the specified type.
            /// </summary>
            UnmatchedFormat,
            /// <summary>
            /// Image is not a square.
            /// </summary>
            BadSize
        }

        public AvatarDataException(Avatar avatar, ErrorReason error, string message) : base(message) { Avatar = avatar; Error = error; }
        public AvatarDataException(Avatar avatar, ErrorReason error, string message, Exception inner) : base(message, inner) { Avatar = avatar; Error = error; }
        protected AvatarDataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public ErrorReason Error { get; set; }
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
        /// Get the etag of default avatar.
        /// </summary>
        /// <returns></returns>
        Task<string> GetDefaultAvatarETag();

        /// <summary>
        /// Get the default avatar.
        /// </summary>
        Task<AvatarInfo> GetDefaultAvatar();
    }

    public interface IUserAvatarValidator
    {
        /// <summary>
        /// Validate a avatar's format and size info.
        /// </summary>
        /// <param name="avatar">The avatar to validate.</param>
        /// <exception cref="AvatarDataException">Thrown when validation failed.</exception>
        Task Validate(Avatar avatar);
    }

    public interface IUserAvatarService
    {
        /// <summary>
        /// Get the etag of a user's avatar.
        /// </summary>
        /// <param name="username">The username of the user to get avatar etag of.</param>
        /// <returns>The etag.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="username"/> is null or empty.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user does not exist.</exception>
        Task<string> GetAvatarETag(string username);

        /// <summary>
        /// Get avatar of a user. If the user has no avatar, a default one is returned.
        /// </summary>
        /// <param name="username">The username of the user to get avatar of.</param>
        /// <returns>The avatar info.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="username"/> is null or empty.</exception>
        /// <exception cref="UserNotExistException">Thrown if the user does not exist.</exception>
        Task<AvatarInfo> GetAvatar(string username);

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

        private readonly IETagGenerator _eTagGenerator;

        private byte[] _cacheData;
        private DateTime _cacheLastModified;
        private string _cacheETag;

        public DefaultUserAvatarProvider(IHostingEnvironment environment, IETagGenerator eTagGenerator)
        {
            _environment = environment;
            _eTagGenerator = eTagGenerator;
        }

        private async Task CheckAndInit()
        {
            if (_cacheData != null)
                return;

            var path = Path.Combine(_environment.ContentRootPath, "default-avatar.png");
            _cacheData = await File.ReadAllBytesAsync(path);
            _cacheLastModified = File.GetLastWriteTime(path);
            _cacheETag = _eTagGenerator.Generate(_cacheData);
        }

        public async Task<string> GetDefaultAvatarETag()
        {
            await CheckAndInit();
            return _cacheETag;
        }

        public async Task<AvatarInfo> GetDefaultAvatar()
        {
            await CheckAndInit();
            return new AvatarInfo
            {
                Avatar = new Avatar
                {
                    Type = "image/png",
                    Data = _cacheData
                },
                LastModified = _cacheLastModified
            };
        }
    }

    public class UserAvatarValidator : IUserAvatarValidator
    {
        public Task Validate(Avatar avatar)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var image = Image.Load(avatar.Data, out IImageFormat format))
                    {
                        if (!format.MimeTypes.Contains(avatar.Type))
                            throw new AvatarDataException(avatar, AvatarDataException.ErrorReason.UnmatchedFormat, "Image's actual mime type is not the specified one.");
                        if (image.Width != image.Height)
                            throw new AvatarDataException(avatar, AvatarDataException.ErrorReason.BadSize, "Image is not a square, aka, width is not equal to height.");
                    }
                }
                catch (UnknownImageFormatException e)
                {
                    throw new AvatarDataException(avatar, AvatarDataException.ErrorReason.CantDecode, "Failed to decode image. See inner exception.", e);
                }
            });
        }
    }

    public class UserAvatarService : IUserAvatarService
    {

        private readonly ILogger<UserAvatarService> _logger;

        private readonly DatabaseContext _database;

        private readonly IDefaultUserAvatarProvider _defaultUserAvatarProvider;
        private readonly IUserAvatarValidator _avatarValidator;

        private readonly IETagGenerator _eTagGenerator;

        public UserAvatarService(
            ILogger<UserAvatarService> logger,
            DatabaseContext database,
            IDefaultUserAvatarProvider defaultUserAvatarProvider,
            IUserAvatarValidator avatarValidator,
            IETagGenerator eTagGenerator)
        {
            _logger = logger;
            _database = database;
            _defaultUserAvatarProvider = defaultUserAvatarProvider;
            _avatarValidator = avatarValidator;
            _eTagGenerator = eTagGenerator;
        }

        public async Task<string> GetAvatarETag(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username is null or empty.", nameof(username));

            var userId = await _database.Users.Where(u => u.Name == username).Select(u => u.Id).SingleOrDefaultAsync();
            if (userId == 0)
                throw new UserNotExistException(username);

            var eTag = (await _database.UserAvatars.Where(a => a.UserId == userId).Select(a => new { a.ETag }).SingleAsync()).ETag;
            if (eTag == null)
                return await _defaultUserAvatarProvider.GetDefaultAvatarETag();
            else
                return eTag;
        }

        public async Task<AvatarInfo> GetAvatar(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username is null or empty.", nameof(username));

            var userId = await _database.Users.Where(u => u.Name == username).Select(u => u.Id).SingleOrDefaultAsync();
            if (userId == 0)
                throw new UserNotExistException(username);

            var avatar = await _database.UserAvatars.Where(a => a.UserId == userId).Select(a => new { a.Type, a.Data, a.LastModified }).SingleAsync();

            if ((avatar.Type == null) != (avatar.Data == null))
            {
                _logger.LogCritical("Database corupted! One of type and data of a avatar is null but the other is not.");
                throw new DatabaseCorruptedException();
            }

            if (avatar.Data == null)
            {
                var defaultAvatar = await _defaultUserAvatarProvider.GetDefaultAvatar();
                defaultAvatar.LastModified = defaultAvatar.LastModified > avatar.LastModified ? defaultAvatar.LastModified : avatar.LastModified;
                return defaultAvatar;
            }
            else
            {
                return new AvatarInfo
                {
                    Avatar = new Avatar
                    {
                        Type = avatar.Type,
                        Data = avatar.Data
                    },
                    LastModified = avatar.LastModified
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

            var userId = await _database.Users.Where(u => u.Name == username).Select(u => u.Id).SingleOrDefaultAsync();
            if (userId == 0)
                throw new UserNotExistException(username);

            var avatarEntity = await _database.UserAvatars.Where(a => a.UserId == userId).SingleAsync();

            if (avatar == null)
            {
                if (avatarEntity.Data == null)
                    return;
                else
                {
                    avatarEntity.Data = null;
                    avatarEntity.Type = null;
                    avatarEntity.ETag = null;
                    avatarEntity.LastModified = DateTime.Now;
                    await _database.SaveChangesAsync();
                    _logger.LogInformation("Updated an entry in user_avatars.");
                }
            }
            else
            {
                await _avatarValidator.Validate(avatar);
                avatarEntity.Type = avatar.Type;
                avatarEntity.Data = avatar.Data;
                avatarEntity.ETag = _eTagGenerator.Generate(avatar.Data);
                avatarEntity.LastModified = DateTime.Now;
                await _database.SaveChangesAsync();
                _logger.LogInformation("Updated an entry in user_avatars.");
            }
        }
    }

    public static class UserAvatarServiceCollectionExtensions
    {
        public static void AddUserAvatarService(this IServiceCollection services)
        {
            services.TryAddTransient<IETagGenerator, ETagGenerator>();
            services.AddScoped<IUserAvatarService, UserAvatarService>();
            services.AddSingleton<IDefaultUserAvatarProvider, DefaultUserAvatarProvider>();
            services.AddSingleton<IUserAvatarValidator, UserAvatarValidator>();
        }
    }
}
