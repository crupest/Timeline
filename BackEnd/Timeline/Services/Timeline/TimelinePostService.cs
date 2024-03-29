﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers;
using Timeline.Helpers.Cache;
using Timeline.Models;
using Timeline.Models.Validation;
using Timeline.Services.Data;
using Timeline.Services.Imaging;
using Timeline.Services.User;

namespace Timeline.Services.Timeline
{
    public class TimelinePostService : ITimelinePostService
    {
        private readonly ILogger<TimelinePostService> _logger;
        private readonly DatabaseContext _database;
        private readonly ITimelineService _timelineService;
        private readonly IUserService _userService;
        private readonly IDataManager _dataManager;
        private readonly IImageService _imageValidator;
        private readonly IClock _clock;
        private readonly ColorValidator _colorValidator = new ColorValidator();
        private readonly ColorValidator _colorValidatorAllowEmptyAndDefault = new ColorValidator() { PermitEmpty = true, PermitDefault = true };

        public TimelinePostService(ILogger<TimelinePostService> logger, DatabaseContext database, ITimelineService basicTimelineService, IUserService basicUserService, IDataManager dataManager, IImageService imageValidator, IClock clock)
        {
            _logger = logger;
            _database = database;
            _timelineService = basicTimelineService;
            _userService = basicUserService;
            _dataManager = dataManager;
            _imageValidator = imageValidator;
            _clock = clock;
        }

        private void CheckColor(string color, string paramName, bool allowEmptyOrDefault)
        {
            if (!(allowEmptyOrDefault ? _colorValidatorAllowEmptyAndDefault : _colorValidator).Validate(color, out var message))
                throw new ArgumentException(string.Format(Resource.ExceptionColorInvalid, message), paramName);
        }

        private static EntityNotExistException CreatePostNotExistException(long timelineId, long postId, bool deleted)
        {
            return new EntityNotExistException(EntityTypes.TimelinePost, new Dictionary<string, object>
            {
                ["timeline-id"] = timelineId,
                ["post-id"] = postId,
                ["deleted"] = deleted
            });
        }

        private static EntityDeletedException CreatePostDeletedException(long timelineId, long postId)
        {
            return new EntityDeletedException(EntityTypes.TimelinePost, new Dictionary<string, object>
            {
                ["timeline-id"] = timelineId,
                ["post-id"] = postId,
            });
        }

        private static EntityNotExistException CreatePostDataNotExistException(long timelineId, long postId, long dataIndex)
        {
            return new EntityNotExistException(EntityTypes.TimelinePost, new Dictionary<string, object>
            {
                ["timeline-id"] = timelineId,
                ["post-id"] = postId,
                ["data-index"] = dataIndex
            });
        }

        public async Task<List<TimelinePostEntity>> GetPostsAsync(long timelineId, DateTime? modifiedSince = null, bool includeDeleted = false, int? page = null, int? numberPerPage = null)
        {
            if (page.HasValue && page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), Resource.ExceptionPageNegative);
            if (numberPerPage.HasValue && numberPerPage <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberPerPage), Resource.ExceptionNumberPerPageZeroOrNegative);


            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            modifiedSince = modifiedSince?.MyToUtc();

            IQueryable<TimelinePostEntity> query = _database.TimelinePosts.Where(p => p.TimelineId == timelineId);

            if (!includeDeleted)
            {
                query = query.Where(p => !p.Deleted);
            }

            if (modifiedSince.HasValue)
            {
                query = query.Where(p => p.LastUpdated >= modifiedSince || (p.Author != null && p.Author.UsernameChangeTime >= modifiedSince));
            }

            query = query.OrderBy(p => p.Time);

            if (page.HasValue)
            {
                var npp = numberPerPage.GetValueOrDefault(20);
                query = query.Skip(npp * (page.Value - 1)).Take(npp);
            }

            return await query.ToListAsync();
        }

        public async Task<TimelinePostEntity> GetPostAsync(long timelineId, long postId, bool includeDeleted = false)
        {
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            var post = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).SingleOrDefaultAsync();

            if (post is null)
            {
                throw CreatePostNotExistException(timelineId, postId, false);
            }

            if (!includeDeleted && post.Deleted)
            {
                throw CreatePostNotExistException(timelineId, postId, true);
            }

            return post;
        }

        public async Task<ICacheableDataDigest> GetPostDataDigestAsync(long timelineId, long postId, long dataIndex)
        {
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            var postEntity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).Select(p => new { p.Id, p.Deleted }).SingleOrDefaultAsync();

            if (postEntity is null)
                throw CreatePostNotExistException(timelineId, postId, false);

            if (postEntity.Deleted)
                throw CreatePostNotExistException(timelineId, postId, true);

            var dataEntity = await _database.TimelinePostData.Where(d => d.PostId == postEntity.Id && d.Index == dataIndex).SingleOrDefaultAsync();

            if (dataEntity is null)
                throw CreatePostDataNotExistException(timelineId, postId, dataIndex);

            return new CacheableDataDigest(dataEntity.DataTag, dataEntity.LastUpdated);
        }

        public async Task<ByteData> GetPostDataAsync(long timelineId, long postId, long dataIndex)
        {
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            var postEntity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).Select(p => new { p.Id, p.Deleted }).SingleOrDefaultAsync();

            if (postEntity is null)
                throw CreatePostNotExistException(timelineId, postId, false);

            if (postEntity.Deleted)
                throw CreatePostNotExistException(timelineId, postId, true);

            var dataEntity = await _database.TimelinePostData.Where(d => d.PostId == postEntity.Id && d.Index == dataIndex).SingleOrDefaultAsync();

            if (dataEntity is null)
                throw CreatePostDataNotExistException(timelineId, postId, dataIndex);

            var data = await _dataManager.GetEntryAndCheck(dataEntity.DataTag, $"Timeline {timelineId}, post {postId}, data {dataIndex} requires this data.");

            return new ByteData(data, dataEntity.Kind);
        }

        public async Task<TimelinePostEntity> CreatePostAsync(long timelineId, long authorId, TimelinePostCreateRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.Color is not null)
                CheckColor(request.Color, nameof(request), false);

            if (request.DataList is null)
                throw new ArgumentException(Resource.ExceptionDataListNull, nameof(request));

            if (request.DataList.Count == 0)
                throw new ArgumentException(Resource.ExceptionDataListEmpty, nameof(request));

            if (request.DataList.Count > 100)
                throw new ArgumentException(Resource.ExceptionDataListTooLarge, nameof(request));

            for (int index = 0; index < request.DataList.Count; index++)
            {
                var data = request.DataList[index];

                switch (data.ContentType)
                {
                    case MimeTypes.ImageGif:
                    case MimeTypes.ImageJpeg:
                    case MimeTypes.ImagePng:
                    case MimeTypes.ImageWebp:
                        try
                        {
                            await _imageValidator.ValidateAsync(data.Data, data.ContentType);
                        }
                        catch (ImageException e)
                        {
                            throw new TimelinePostCreateDataException(index, Resource.ExceptionPostDataImageInvalid, e);
                        }
                        break;
                    case MimeTypes.TextPlain:
                    case MimeTypes.TextMarkdown:
                        try
                        {
                            new UTF8Encoding(false, true).GetString(data.Data);
                        }
                        catch (DecoderFallbackException e)
                        {
                            throw new TimelinePostCreateDataException(index, Resource.ExceptionPostDataNotValidUtf8, e);
                        }
                        break;
                    default:
                        throw new TimelinePostCreateDataException(index, Resource.ExceptionPostDataUnsupportedType);
                }
            }

            request.Time = request.Time?.MyToUtc();

            await _timelineService.ThrowIfTimelineNotExist(timelineId);
            await _userService.ThrowIfUserNotExist(authorId);

            var currentTime = _clock.GetCurrentTime();
            var finalTime = request.Time ?? currentTime;

            await using var transaction = await _database.Database.BeginTransactionAsync();

            var postEntity = new TimelinePostEntity
            {
                AuthorId = authorId,
                TimelineId = timelineId,
                Time = finalTime,
                LastUpdated = currentTime,
                Color = request.Color
            };

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).SingleAsync();
            timelineEntity.CurrentPostLocalId += 1;
            postEntity.LocalId = timelineEntity.CurrentPostLocalId;
            _database.TimelinePosts.Add(postEntity);
            await _database.SaveChangesAsync();

            List<string> dataTags = new List<string>();

            for (int index = 0; index < request.DataList.Count; index++)
            {
                var data = request.DataList[index];

                var tag = await _dataManager.RetainEntryAsync(data.Data);

                _database.TimelinePostData.Add(new TimelinePostDataEntity
                {
                    DataTag = tag,
                    Kind = data.ContentType,
                    Index = index,
                    PostId = postEntity.Id,
                    LastUpdated = currentTime,
                });
            }

            await _database.SaveChangesAsync();

            await transaction.CommitAsync();
            _logger.LogInformation(Resource.LogTimelinePostCreated, timelineId, postEntity.Id);

            return postEntity;
        }

        public async Task<TimelinePostEntity> PatchPostAsync(long timelineId, long postId, TimelinePostPatchRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.Color is not null)
                CheckColor(request.Color, nameof(request), true);

            request.Time = request.Time?.MyToUtc();

            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            var entity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).SingleOrDefaultAsync();

            if (entity is null)
                throw CreatePostNotExistException(timelineId, postId, false);

            if (entity.Deleted)
                throw CreatePostNotExistException(timelineId, postId, true);

            if (request.Time.HasValue)
                entity.Time = request.Time.Value;

            if (request.Color is not null)
            {
                if (request.Color.Length == 0 || request.Color == "default")
                {
                    entity.Color = null;
                }
                else
                {
                    entity.Color = request.Color;
                }
            }

            entity.LastUpdated = _clock.GetCurrentTime();

            await _database.SaveChangesAsync();
            _logger.LogInformation(Resource.LogTimelinePostUpdated, timelineId, postId);

            return entity;
        }

        public async Task DeletePostAsync(long timelineId, long postId)
        {
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            var entity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).SingleOrDefaultAsync();

            if (entity == null)
                throw CreatePostNotExistException(timelineId, postId, false);

            if (entity.Deleted)
                throw CreatePostNotExistException(timelineId, postId, true);

            await using var transaction = await _database.Database.BeginTransactionAsync();

            entity.Deleted = true;
            entity.LastUpdated = _clock.GetCurrentTime();

            var dataEntities = await _database.TimelinePostData.Where(d => d.PostId == entity.Id).ToListAsync();

            foreach (var dataEntity in dataEntities)
            {
                await _dataManager.FreeEntryAsync(dataEntity.DataTag);
            }

            _database.TimelinePostData.RemoveRange(dataEntities);

            await _database.SaveChangesAsync();

            await transaction.CommitAsync();
            _logger.LogWarning(Resource.LogTimelinePostDeleted, timelineId, postId);
        }

        public async Task DeleteAllPostsOfUserAsync(long userId)
        {
            var postEntities = await _database.TimelinePosts.Where(p => p.AuthorId == userId).Select(p => new { p.TimelineId, p.LocalId }).ToListAsync();

            foreach (var postEntity in postEntities)
            {
                await this.DeletePostAsync(postEntity.TimelineId, postEntity.LocalId);
            }
        }

        public async Task<bool> HasPostModifyPermissionAsync(long timelineId, long postId, long modifierId, bool throwOnPostNotExist = false)
        {
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleAsync();

            var postEntity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).Select(p => new { p.Deleted, p.AuthorId }).SingleOrDefaultAsync();

            if (postEntity is null)
            {
                if (throwOnPostNotExist)
                    throw CreatePostNotExistException(timelineId, postId, false);
                else
                    return true;
            }

            if (postEntity.Deleted && throwOnPostNotExist)
            {
                throw CreatePostNotExistException(timelineId, postId, true);
            }

            return timelineEntity.OwnerId == modifierId || postEntity.AuthorId == modifierId;
        }

        public async Task<Page<TimelinePostEntity>> GetPostsV2Async(long timelineId, int page, int pageSize, DateTime? modifiedSince = null)
        {
            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page));
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            var timeline = await _timelineService.GetTimelineAsync(timelineId);

            modifiedSince = modifiedSince?.MyToUtc();

            IQueryable<TimelinePostEntity> query = _database.TimelinePosts.Where(p => p.TimelineId == timelineId);

            if (modifiedSince.HasValue)
            {
                query = query.Where(p => p.LastUpdated >= modifiedSince || (p.Author != null && p.Author.UsernameChangeTime >= modifiedSince));
            }

            query = query.OrderByDescending(p => p.Time).Skip(pageSize * (page - 1)).Take(pageSize);

            var items = await query.ToListAsync();

            return new Page<TimelinePostEntity>(page, pageSize, timeline.CurrentPostLocalId, items);
        }

        public async Task<TimelinePostEntity> GetPostV2Async(long timelineId, long postId)
        {
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            var post = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).SingleOrDefaultAsync();

            if (post is null)
            {
                throw CreatePostNotExistException(timelineId, postId, false);
            }

            if (post.Deleted)
            {
                throw CreatePostDeletedException(timelineId, postId);
            }

            return post;
        }

        public async Task<ICacheableDataDigest> GetPostDataDigestV2Async(long timelineId, long postId, long dataIndex)
        {
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            var postEntity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).Select(p => new { p.Id, p.Deleted }).SingleOrDefaultAsync();

            if (postEntity is null)
                throw CreatePostNotExistException(timelineId, postId, false);

            if (postEntity.Deleted)
                throw CreatePostDeletedException(timelineId, postId);

            var dataEntity = await _database.TimelinePostData.Where(d => d.PostId == postEntity.Id && d.Index == dataIndex).SingleOrDefaultAsync();

            if (dataEntity is null)
                throw CreatePostDataNotExistException(timelineId, postId, dataIndex);

            return new CacheableDataDigest(dataEntity.DataTag, dataEntity.LastUpdated);
        }

        public async Task<ByteData> GetPostDataV2Async(long timelineId, long postId, long dataIndex)
        {
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            var postEntity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).Select(p => new { p.Id, p.Deleted }).SingleOrDefaultAsync();

            if (postEntity is null)
                throw CreatePostNotExistException(timelineId, postId, false);

            if (postEntity.Deleted)
                throw CreatePostDeletedException(timelineId, postId);

            var dataEntity = await _database.TimelinePostData.Where(d => d.PostId == postEntity.Id && d.Index == dataIndex).SingleOrDefaultAsync();

            if (dataEntity is null)
                throw CreatePostDataNotExistException(timelineId, postId, dataIndex);

            var data = await _dataManager.GetEntryAndCheck(dataEntity.DataTag, $"Timeline {timelineId}, post {postId}, data {dataIndex} requires this data.");

            return new ByteData(data, dataEntity.Kind);
        }
    }
}
