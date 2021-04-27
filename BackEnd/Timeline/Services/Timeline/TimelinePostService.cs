using Microsoft.EntityFrameworkCore;
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
    public class TimelinePostCreateRequestData
    {
        public TimelinePostCreateRequestData(string contentType, byte[] data)
        {
            ContentType = contentType;
            Data = data;
        }

        public string ContentType { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    }

    public class TimelinePostCreateRequest
    {
        public string? Color { get; set; }

        /// <summary>If not set, current time is used.</summary>
        public DateTime? Time { get; set; }

#pragma warning disable CA2227
        public List<TimelinePostCreateRequestData> DataList { get; set; } = new List<TimelinePostCreateRequestData>();
#pragma warning restore CA2227
    }

    public class TimelinePostPatchRequest
    {
        public string? Color { get; set; }
        public DateTime? Time { get; set; }
    }

    public interface ITimelinePostService
    {
        /// <summary>
        /// Get all the posts in the timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="modifiedSince">The time that posts have been modified since.</param>
        /// <param name="includeDeleted">Whether include deleted posts.</param>
        /// <returns>A list of all posts.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        Task<List<TimelinePostEntity>> GetPosts(long timelineId, DateTime? modifiedSince = null, bool includeDeleted = false);

        /// <summary>
        /// Get a post of a timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline of the post.</param>
        /// <param name="postId">The id of the post.</param>
        /// <param name="includeDeleted">If true, return the entity even if it is deleted.</param>
        /// <returns>The post.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="TimelinePostNotExistException">Thrown when post of <paramref name="postId"/> does not exist or has been deleted.</exception>
        Task<TimelinePostEntity> GetPost(long timelineId, long postId, bool includeDeleted = false);

        /// <summary>
        /// Get the data digest of a post.
        /// </summary>
        /// <param name="timelineId">The timeline id.</param>
        /// <param name="postId">The post id.</param>
        /// <param name="dataIndex">The index of the data.</param>
        /// <returns>The data digest.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="TimelinePostNotExistException">Thrown when post of <paramref name="postId"/> does not exist or has been deleted.</exception>
        /// <exception cref="TimelinePostDataNotExistException">Thrown when data of that index does not exist.</exception>
        Task<ICacheableDataDigest> GetPostDataDigest(long timelineId, long postId, long dataIndex);

        /// <summary>
        /// Get the data of a post.
        /// </summary>
        /// <param name="timelineId">The timeline id.</param>
        /// <param name="postId">The post id.</param>
        /// <param name="dataIndex">The index of the data.</param>
        /// <returns>The data.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="TimelinePostNotExistException">Thrown when post of <paramref name="postId"/> does not exist or has been deleted.</exception>
        /// <exception cref="TimelinePostDataNotExistException">Thrown when data of that index does not exist.</exception>
        Task<ByteData> GetPostData(long timelineId, long postId, long dataIndex);

        /// <summary>
        /// Create a new post in timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline to create post against.</param>
        /// <param name="authorId">The author's user id.</param>
        /// <param name="request">Info about the post.</param>
        /// <returns>The entity of the created post.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="request"/> is of invalid format.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown if user of <paramref name="authorId"/> does not exist.</exception>
        /// <exception cref="ImageException">Thrown if data is not a image. Validated by <see cref="ImageService"/>.</exception>
        Task<TimelinePostEntity> CreatePost(long timelineId, long authorId, TimelinePostCreateRequest request);

        /// <summary>
        /// Modify a post. Change its properties or replace its content.
        /// </summary>
        /// <param name="timelineId">The timeline id.</param>
        /// <param name="postId">The post id.</param>
        /// <param name="request">The request.</param>
        /// <returns>The entity of the patched post.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="request"/> is of invalid format.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="TimelinePostNotExistException">Thrown when post does not exist.</exception>
        Task<TimelinePostEntity> PatchPost(long timelineId, long postId, TimelinePostPatchRequest request);

        /// <summary>
        /// Delete a post.
        /// </summary>
        /// <param name="timelineId">The id of the timeline to delete post against.</param>
        /// <param name="postId">The id of the post to delete.</param>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="TimelinePostNotExistException">Thrown when the post with given id does not exist or is deleted already.</exception>
        /// <remarks>
        /// First use <see cref="HasPostModifyPermission(long, long, long, bool)"/> to check the permission.
        /// </remarks>
        Task DeletePost(long timelineId, long postId);

        /// <summary>
        /// Delete all posts of the given user. Used when delete a user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        Task DeleteAllPostsOfUser(long userId);

        /// <summary>
        /// Verify whether a user has the permission to modify a post.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="postId">The id of the post.</param>
        /// <param name="modifierId">The id of the user to check on.</param>
        /// <param name="throwOnPostNotExist">True if you want it to throw <see cref="TimelinePostNotExistException"/>. Default false.</param>
        /// <returns>True if can modify, false if can't modify.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="TimelinePostNotExistException">Thrown when the post with given id does not exist or is deleted already and <paramref name="throwOnPostNotExist"/> is true.</exception>
        /// <remarks>
        /// Unless <paramref name="throwOnPostNotExist"/> is true, this method should return true if the post does not exist.
        /// If the post is deleted, its author info still exists, so it is checked as the post is not deleted unless <paramref name="throwOnPostNotExist"/> is true.
        /// This method does not check whether the user is administrator.
        /// It only checks whether he is the author of the post or the owner of the timeline.
        /// Return false when user with modifier id does not exist.
        /// </remarks>
        Task<bool> HasPostModifyPermission(long timelineId, long postId, long modifierId, bool throwOnPostNotExist = false);
    }

    public class TimelinePostService : ITimelinePostService
    {
        private readonly ILogger<TimelinePostService> _logger;
        private readonly DatabaseContext _database;
        private readonly IBasicTimelineService _basicTimelineService;
        private readonly IBasicUserService _basicUserService;
        private readonly IDataManager _dataManager;
        private readonly IImageService _imageValidator;
        private readonly IClock _clock;
        private readonly ColorValidator _colorValidator = new ColorValidator();

        public TimelinePostService(ILogger<TimelinePostService> logger, DatabaseContext database, IBasicTimelineService basicTimelineService, IBasicUserService basicUserService, IDataManager dataManager, IImageService imageValidator, IClock clock)
        {
            _logger = logger;
            _database = database;
            _basicTimelineService = basicTimelineService;
            _basicUserService = basicUserService;
            _dataManager = dataManager;
            _imageValidator = imageValidator;
            _clock = clock;
        }

        private async Task CheckTimelineExistence(long timelineId)
        {
            if (!await _basicTimelineService.CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);
        }

        private async Task CheckUserExistence(long userId)
        {
            if (!await _basicUserService.CheckUserExistenceAsync(userId))
                throw new UserNotExistException(userId);
        }

        public async Task<List<TimelinePostEntity>> GetPosts(long timelineId, DateTime? modifiedSince = null, bool includeDeleted = false)
        {
            await CheckTimelineExistence(timelineId);

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

            return await query.ToListAsync();
        }

        public async Task<TimelinePostEntity> GetPost(long timelineId, long postId, bool includeDeleted = false)
        {
            await CheckTimelineExistence(timelineId);

            var post = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).SingleOrDefaultAsync();

            if (post is null)
            {
                throw new TimelinePostNotExistException(timelineId, postId, false);
            }

            if (!includeDeleted && post.Deleted)
            {
                throw new TimelinePostNotExistException(timelineId, postId, true);
            }

            return post;
        }

        public async Task<ICacheableDataDigest> GetPostDataDigest(long timelineId, long postId, long dataIndex)
        {
            await CheckTimelineExistence(timelineId);

            var postEntity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).Select(p => new { p.Id, p.Deleted }).SingleOrDefaultAsync();

            if (postEntity is null)
                throw new TimelinePostNotExistException(timelineId, postId, false);

            if (postEntity.Deleted)
                throw new TimelinePostNotExistException(timelineId, postId, true);

            var dataEntity = await _database.TimelinePostData.Where(d => d.PostId == postEntity.Id && d.Index == dataIndex).SingleOrDefaultAsync();

            if (dataEntity is null)
                throw new TimelinePostDataNotExistException(timelineId, postId, dataIndex);

            return new CacheableDataDigest(dataEntity.DataTag, dataEntity.LastUpdated);
        }

        public async Task<ByteData> GetPostData(long timelineId, long postId, long dataIndex)
        {
            await CheckTimelineExistence(timelineId);

            var postEntity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).Select(p => new { p.Id, p.Deleted }).SingleOrDefaultAsync();

            if (postEntity is null)
                throw new TimelinePostNotExistException(timelineId, postId, false);

            if (postEntity.Deleted)
                throw new TimelinePostNotExistException(timelineId, postId, true);

            var dataEntity = await _database.TimelinePostData.Where(d => d.PostId == postEntity.Id && d.Index == dataIndex).SingleOrDefaultAsync();

            if (dataEntity is null)
                throw new TimelinePostDataNotExistException(timelineId, postId, dataIndex);

            var data = await _dataManager.GetEntryAndCheck(dataEntity.DataTag, $"Timeline {timelineId}, post {postId}, data {dataIndex} requires this data.");

            return new ByteData(data, dataEntity.Kind);
        }

        public async Task<TimelinePostEntity> CreatePost(long timelineId, long authorId, TimelinePostCreateRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            {
                if (!_colorValidator.Validate(request.Color, out var message))
                    throw new ArgumentException("Color is not valid.", nameof(request));
            }

            if (request.DataList is null)
                throw new ArgumentException("Data list can't be null.", nameof(request));

            if (request.DataList.Count == 0)
                throw new ArgumentException("Data list can't be empty.", nameof(request));

            if (request.DataList.Count > 100)
                throw new ArgumentException("Data list count can't be bigger than 100.", nameof(request));

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
                            throw new TimelinePostCreateDataException(index, "Image validation failed.", e);
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
                            throw new TimelinePostCreateDataException(index, "Text is not a valid utf-8 sequence.", e);
                        }
                        break;
                    default:
                        throw new TimelinePostCreateDataException(index, "Unsupported content type.");
                }
            }

            request.Time = request.Time?.MyToUtc();

            await CheckTimelineExistence(timelineId);
            await CheckUserExistence(authorId);

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

            return postEntity;
        }

        public async Task<TimelinePostEntity> PatchPost(long timelineId, long postId, TimelinePostPatchRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            {
                if (!_colorValidator.Validate(request.Color, out var message))
                    throw new ArgumentException("Color is not valid.", nameof(request));
            }

            request.Time = request.Time?.MyToUtc();

            await CheckTimelineExistence(timelineId);

            var entity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelinePostNotExistException(timelineId, postId, false);

            if (entity.Deleted)
                throw new TimelinePostNotExistException(timelineId, postId, true);

            if (request.Time.HasValue)
                entity.Time = request.Time.Value;

            if (request.Color is not null)
                entity.Color = request.Color;

            entity.LastUpdated = _clock.GetCurrentTime();

            await _database.SaveChangesAsync();

            return entity;
        }

        public async Task DeletePost(long timelineId, long postId)
        {
            await CheckTimelineExistence(timelineId);

            var entity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).SingleOrDefaultAsync();

            if (entity == null)
                throw new TimelinePostNotExistException(timelineId, postId, false);

            if (entity.Deleted)
                throw new TimelinePostNotExistException(timelineId, postId, true);

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
        }

        public async Task DeleteAllPostsOfUser(long userId)
        {
            var postEntities = await _database.TimelinePosts.Where(p => p.AuthorId == userId).Select(p => new { p.TimelineId, p.LocalId }).ToListAsync();

            foreach (var postEntity in postEntities)
            {
                await this.DeletePost(postEntity.TimelineId, postEntity.LocalId);
            }
        }

        public async Task<bool> HasPostModifyPermission(long timelineId, long postId, long modifierId, bool throwOnPostNotExist = false)
        {
            await CheckTimelineExistence(timelineId);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleAsync();

            var postEntity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).Select(p => new { p.Deleted, p.AuthorId }).SingleOrDefaultAsync();

            if (postEntity is null)
            {
                if (throwOnPostNotExist)
                    throw new TimelinePostNotExistException(timelineId, postId, false);
                else
                    return true;
            }

            if (postEntity.Deleted && throwOnPostNotExist)
            {
                throw new TimelinePostNotExistException(timelineId, postId, true);
            }

            return timelineEntity.OwnerId == modifierId || postEntity.AuthorId == modifierId;
        }
    }
}
