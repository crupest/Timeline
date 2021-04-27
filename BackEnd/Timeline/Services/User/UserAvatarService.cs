using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers.Cache;
using Timeline.Models;
using Timeline.Services.Data;
using Timeline.Services.Imaging;

namespace Timeline.Services.User
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

    public interface IUserAvatarService
    {
        /// <summary>
        /// Get avatar digest of a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>The avatar digest.</returns>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        Task<ICacheableDataDigest> GetAvatarDigest(long userId);

        /// <summary>
        /// Get avatar of a user. If the user has no avatar set, a default one is returned.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>The avatar.</returns>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        Task<ByteData> GetAvatar(long userId);

        /// <summary>
        /// Set avatar for a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="avatar">The new avatar data.</param>
        /// <returns>The digest of the avatar.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="avatar"/> is null.</exception>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="ImageException">Thrown if avatar is of bad format.</exception>
        Task<ICacheableDataDigest> SetAvatar(long userId, ByteData avatar);

        /// <summary>
        /// Remove avatar of a user.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        Task DeleteAvatar(long userId);
    }

    // TODO! : Make this configurable.
    public class DefaultUserAvatarProvider : IDefaultUserAvatarProvider
    {
        private readonly IETagGenerator _eTagGenerator;

        private readonly string _avatarPath;

        private CacheableDataDigest? _cacheDigest;
        private ByteData? _cacheData;

        public DefaultUserAvatarProvider(IWebHostEnvironment environment, IETagGenerator eTagGenerator)
        {
            _avatarPath = Path.Combine(environment.ContentRootPath, "default-avatar.png");
            _eTagGenerator = eTagGenerator;
        }

        private async Task CheckAndInit()
        {
            var path = _avatarPath;
            if (_cacheData == null || File.GetLastWriteTime(path) > _cacheDigest!.LastModified)
            {
                var data = await File.ReadAllBytesAsync(path);
                _cacheDigest = new CacheableDataDigest(await _eTagGenerator.GenerateETagAsync(data), File.GetLastWriteTime(path));
                Image.Identify(data, out var format);
                _cacheData = new ByteData(data, format.DefaultMimeType);
            }
        }

        public async Task<ICacheableDataDigest> GetDefaultAvatarDigest()
        {
            await CheckAndInit();
            return _cacheDigest!;
        }

        public async Task<ByteData> GetDefaultAvatar()
        {
            await CheckAndInit();
            return _cacheData!;
        }
    }

    public class UserAvatarService : IUserAvatarService
    {
        private readonly ILogger<UserAvatarService> _logger;
        private readonly DatabaseContext _database;
        private readonly IBasicUserService _basicUserService;
        private readonly IDefaultUserAvatarProvider _defaultUserAvatarProvider;
        private readonly IImageService _imageValidator;
        private readonly IDataManager _dataManager;
        private readonly IClock _clock;

        public UserAvatarService(
            ILogger<UserAvatarService> logger,
            DatabaseContext database,
            IBasicUserService basicUserService,
            IDefaultUserAvatarProvider defaultUserAvatarProvider,
            IImageService imageValidator,
            IDataManager dataManager,
            IClock clock)
        {
            _logger = logger;
            _database = database;
            _basicUserService = basicUserService;
            _defaultUserAvatarProvider = defaultUserAvatarProvider;
            _imageValidator = imageValidator;
            _dataManager = dataManager;
            _clock = clock;
        }

        public async Task<ICacheableDataDigest> GetAvatarDigest(long userId)
        {
            var usernameChangeTime = await _basicUserService.GetUsernameLastModifiedTimeAsync(userId);

            var entity = await _database.UserAvatars.Where(a => a.UserId == userId).Select(a => new { a.DataTag, a.LastModified }).SingleOrDefaultAsync();

            if (entity is null)
            {
                var defaultAvatarDigest = await _defaultUserAvatarProvider.GetDefaultAvatarDigest();
                return new CacheableDataDigest(defaultAvatarDigest.ETag, new DateTime[] { usernameChangeTime, defaultAvatarDigest.LastModified }.Max());
            }
            else if (entity.DataTag is null)
            {
                var defaultAvatarDigest = await _defaultUserAvatarProvider.GetDefaultAvatarDigest();
                return new CacheableDataDigest(defaultAvatarDigest.ETag, new DateTime[] { usernameChangeTime, defaultAvatarDigest.LastModified, entity.LastModified }.Max());
            }
            else
            {
                return new CacheableDataDigest(entity.DataTag, new DateTime[] { usernameChangeTime, entity.LastModified }.Max());
            }
        }

        public async Task<ByteData> GetAvatar(long userId)
        {
            await _basicUserService.ThrowIfUserNotExist(userId);

            var entity = await _database.UserAvatars.Where(a => a.UserId == userId).SingleOrDefaultAsync();

            if (entity is null || entity.DataTag is null)
            {
                return await _defaultUserAvatarProvider.GetDefaultAvatar();
            }

            var data = await _dataManager.GetEntryAndCheck(entity.DataTag, $"This is required by avatar of {userId}.");

            if (entity.Type is null)
            {
                Image.Identify(data, out var format);
                entity.Type = format.DefaultMimeType;
                await _database.SaveChangesAsync();
            }

            return new ByteData(data, entity.Type);
        }

        public async Task<ICacheableDataDigest> SetAvatar(long userId, ByteData avatar)
        {
            if (avatar is null)
                throw new ArgumentNullException(nameof(avatar));

            await _imageValidator.ValidateAsync(avatar.Data, avatar.ContentType, true);

            await _basicUserService.ThrowIfUserNotExist(userId);

            var entity = await _database.UserAvatars.Where(a => a.UserId == userId).SingleOrDefaultAsync();

            await using var transaction = await _database.Database.BeginTransactionAsync();

            var tag = await _dataManager.RetainEntryAsync(avatar.Data);

            var now = _clock.GetCurrentTime();

            if (entity is null)
            {
                var newEntity = new UserAvatarEntity
                {
                    DataTag = tag,
                    Type = avatar.ContentType,
                    LastModified = now,
                    UserId = userId
                };
                _database.Add(newEntity);
            }
            else
            {
                if (entity.DataTag is not null)
                    await _dataManager.FreeEntryAsync(entity.DataTag);

                entity.DataTag = tag;
                entity.Type = avatar.ContentType;
                entity.LastModified = now;
            }

            await _database.SaveChangesAsync();

            await transaction.CommitAsync();

            return new CacheableDataDigest(tag, now);
        }

        public async Task DeleteAvatar(long userId)
        {
            await _basicUserService.ThrowIfUserNotExist(userId);

            var entity = await _database.UserAvatars.Where(a => a.UserId == userId).SingleOrDefaultAsync();

            if (entity is null || entity.DataTag is null)
                return;

            await using var transaction = await _database.Database.BeginTransactionAsync();

            await _dataManager.FreeEntryAsync(entity.DataTag);

            entity.DataTag = null;
            entity.Type = null;
            entity.LastModified = _clock.GetCurrentTime();

            await _database.SaveChangesAsync();

            await transaction.CommitAsync();
        }
    }

    public static class UserAvatarServiceCollectionExtensions
    {
        public static void AddUserAvatarService(this IServiceCollection services)
        {
            services.AddScoped<IUserAvatarService, UserAvatarService>();
            services.AddScoped<IDefaultUserAvatarProvider, DefaultUserAvatarProvider>();
        }
    }
}
