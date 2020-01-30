﻿using Microsoft.AspNetCore.Hosting;
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
using Timeline.Helpers;

namespace Timeline.Services
{
    public class Avatar
    {
        public string Type { get; set; } = default!;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "DTO Object")]
        public byte[] Data { get; set; } = default!;
    }

    public class AvatarInfo
    {
        public Avatar Avatar { get; set; } = default!;
        public DateTime LastModified { get; set; }
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
        /// <exception cref="AvatarFormatException">Thrown when validation failed.</exception>
        Task Validate(Avatar avatar);
    }

    public interface IUserAvatarService
    {
        /// <summary>
        /// Get the etag of a user's avatar. Warning: This method does not check the user existence.
        /// </summary>
        /// <param name="id">The id of the user to get avatar etag of.</param>
        /// <returns>The etag.</returns>
        Task<string> GetAvatarETag(long id);

        /// <summary>
        /// Get avatar of a user. If the user has no avatar set, a default one is returned. Warning: This method does not check the user existence.
        /// </summary>
        /// <param name="id">The id of the user to get avatar of.</param>
        /// <returns>The avatar info.</returns>
        Task<AvatarInfo> GetAvatar(long id);

        /// <summary>
        /// Set avatar for a user. Warning: This method does not check the user existence.
        /// </summary>
        /// <param name="id">The id of the user to set avatar for.</param>
        /// <param name="avatar">The avatar. Can be null to delete the saved avatar.</param>
        /// <exception cref="ArgumentException">Thrown if any field in <paramref name="avatar"/> is null when <paramref name="avatar"/> is not null.</exception>
        /// <exception cref="AvatarFormatException">Thrown if avatar is of bad format.</exception>
        Task SetAvatar(long id, Avatar? avatar);
    }

    // TODO! : Make this configurable.
    public class DefaultUserAvatarProvider : IDefaultUserAvatarProvider
    {
        private readonly IETagGenerator _eTagGenerator;

        private readonly string _avatarPath;

        private byte[] _cacheData = default!;
        private DateTime _cacheLastModified;
        private string _cacheETag = default!;

        public DefaultUserAvatarProvider(IWebHostEnvironment environment, IETagGenerator eTagGenerator)
        {
            _avatarPath = Path.Combine(environment.ContentRootPath, "default-avatar.png");
            _eTagGenerator = eTagGenerator;
        }

        private async Task CheckAndInit()
        {
            var path = _avatarPath;
            if (_cacheData == null || File.GetLastWriteTime(path) > _cacheLastModified)
            {
                _cacheData = await File.ReadAllBytesAsync(path);
                _cacheLastModified = File.GetLastWriteTime(path);
                _cacheETag = await _eTagGenerator.Generate(_cacheData);
            }
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
                    using var image = Image.Load(avatar.Data, out IImageFormat format);
                    if (!format.MimeTypes.Contains(avatar.Type))
                        throw new AvatarFormatException(avatar, AvatarFormatException.ErrorReason.UnmatchedFormat);
                    if (image.Width != image.Height)
                        throw new AvatarFormatException(avatar, AvatarFormatException.ErrorReason.BadSize);
                }
                catch (UnknownImageFormatException e)
                {
                    throw new AvatarFormatException(avatar, AvatarFormatException.ErrorReason.CantDecode, e);
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

        private readonly IClock _clock;

        public UserAvatarService(
            ILogger<UserAvatarService> logger,
            DatabaseContext database,
            IDefaultUserAvatarProvider defaultUserAvatarProvider,
            IUserAvatarValidator avatarValidator,
            IETagGenerator eTagGenerator,
            IClock clock)
        {
            _logger = logger;
            _database = database;
            _defaultUserAvatarProvider = defaultUserAvatarProvider;
            _avatarValidator = avatarValidator;
            _eTagGenerator = eTagGenerator;
            _clock = clock;
        }

        public async Task<string> GetAvatarETag(long id)
        {
            var eTag = (await _database.UserAvatars.Where(a => a.UserId == id).Select(a => new { a.ETag }).SingleOrDefaultAsync())?.ETag;
            if (eTag == null)
                return await _defaultUserAvatarProvider.GetDefaultAvatarETag();
            else
                return eTag;
        }

        public async Task<AvatarInfo> GetAvatar(long id)
        {
            var avatarEntity = await _database.UserAvatars.Where(a => a.UserId == id).Select(a => new { a.Type, a.Data, a.LastModified }).SingleOrDefaultAsync();

            if (avatarEntity != null)
            {
                if (!LanguageHelper.AreSame(avatarEntity.Data == null, avatarEntity.Type == null))
                {
                    var message = Resources.Services.UserAvatarService.ExceptionDatabaseCorruptedDataAndTypeNotSame;
                    _logger.LogCritical(message);
                    throw new DatabaseCorruptedException(message);
                }

                if (avatarEntity.Data != null)
                {
                    return new AvatarInfo
                    {
                        Avatar = new Avatar
                        {
                            Type = avatarEntity.Type!,
                            Data = avatarEntity.Data
                        },
                        LastModified = avatarEntity.LastModified
                    };
                }
            }
            var defaultAvatar = await _defaultUserAvatarProvider.GetDefaultAvatar();
            if (avatarEntity != null)
                defaultAvatar.LastModified = defaultAvatar.LastModified > avatarEntity.LastModified ? defaultAvatar.LastModified : avatarEntity.LastModified;
            return defaultAvatar;
        }

        public async Task SetAvatar(long id, Avatar? avatar)
        {
            if (avatar != null)
            {
                if (avatar.Data == null)
                    throw new ArgumentException(Resources.Services.UserAvatarService.ExceptionAvatarDataNull, nameof(avatar));
                if (string.IsNullOrEmpty(avatar.Type))
                    throw new ArgumentException(Resources.Services.UserAvatarService.ExceptionAvatarTypeNullOrEmpty, nameof(avatar));
            }

            var avatarEntity = await _database.UserAvatars.Where(a => a.UserId == id).SingleOrDefaultAsync();

            if (avatar == null)
            {
                if (avatarEntity == null || avatarEntity.Data == null)
                {
                    return;
                }
                else
                {
                    avatarEntity.Data = null;
                    avatarEntity.Type = null;
                    avatarEntity.ETag = null;
                    avatarEntity.LastModified = _clock.GetCurrentTime();
                    await _database.SaveChangesAsync();
                    _logger.LogInformation(Resources.Services.UserAvatarService.LogUpdateEntity);
                }
            }
            else
            {
                await _avatarValidator.Validate(avatar);
                var create = avatarEntity == null;
                if (create)
                {
                    avatarEntity = new UserAvatarEntity();
                }
                avatarEntity!.Type = avatar.Type;
                avatarEntity.Data = avatar.Data;
                avatarEntity.ETag = await _eTagGenerator.Generate(avatar.Data);
                avatarEntity.LastModified = _clock.GetCurrentTime();
                avatarEntity.UserId = id;
                if (create)
                {
                    _database.UserAvatars.Add(avatarEntity);
                }
                await _database.SaveChangesAsync();
                _logger.LogInformation(create ?
                    Resources.Services.UserAvatarService.LogCreateEntity
                    : Resources.Services.UserAvatarService.LogUpdateEntity);
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
            services.AddTransient<IUserAvatarValidator, UserAvatarValidator>();
        }
    }
}
