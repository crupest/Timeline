using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers.Cache;
using Timeline.Models;
using Timeline.Services.Data;
using Timeline.Services.Imaging;

namespace Timeline.Services.User.Avatar
{
    public class UserAvatarService : IUserAvatarService
    {
        private readonly ILogger<UserAvatarService> _logger;
        private readonly DatabaseContext _database;
        private readonly IBasicUserService _basicUserService;
        private readonly IDefaultUserAvatarProvider _defaultUserAvatarProvider;
        private readonly IImageService _imageService;
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
            _imageService = imageValidator;
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
                var format = await _imageService.DetectFormatAsync(data);
                entity.Type = format.DefaultMimeType;
                await _database.SaveChangesAsync();
            }

            return new ByteData(data, entity.Type);
        }

        public async Task<ICacheableDataDigest> SetAvatar(long userId, ByteData avatar)
        {
            if (avatar is null)
                throw new ArgumentNullException(nameof(avatar));

            await _imageService.ValidateAsync(avatar.Data, avatar.ContentType, true);

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
}
