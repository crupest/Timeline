using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Services.Exceptions;
using SixLabors.ImageSharp;
using static Timeline.Resources.Services.TimelineService;
using Microsoft.Extensions.Logging;

namespace Timeline.Services
{
    public class PostData : ICacheableData
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; set; } = default!;
#pragma warning restore CA1819 // Properties should not return arrays
        public string Type { get; set; } = default!;
        public string ETag { get; set; } = default!;
        public DateTime? LastModified { get; set; } // TODO: Why nullable?
    }

    public interface ITimelinePostService
    {
        /// <summary>
        /// Get all the posts in the timeline.
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <param name="modifiedSince">The time that posts have been modified since.</param>
        /// <param name="includeDeleted">Whether include deleted posts.</param>
        /// <returns>A list of all posts.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        Task<List<TimelinePost>> GetPosts(string timelineName, DateTime? modifiedSince = null, bool includeDeleted = false);

        /// <summary>
        /// Get the etag of data of a post.
        /// </summary>
        /// <param name="timelineName">The name of the timeline of the post.</param>
        /// <param name="postId">The id of the post.</param>
        /// <returns>The etag of the data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="TimelinePostNotExistException">Thrown when post of <paramref name="postId"/> does not exist or has been deleted.</exception>
        /// <exception cref="TimelinePostNoDataException">Thrown when post has no data.</exception>
        /// <seealso cref="GetPostData(string, long)"/>
        Task<string> GetPostDataETag(string timelineName, long postId);

        /// <summary>
        /// Get the data of a post.
        /// </summary>
        /// <param name="timelineName">The name of the timeline of the post.</param>
        /// <param name="postId">The id of the post.</param>
        /// <returns>The etag of the data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="TimelinePostNotExistException">Thrown when post of <paramref name="postId"/> does not exist or has been deleted.</exception>
        /// <exception cref="TimelinePostNoDataException">Thrown when post has no data.</exception>
        /// <seealso cref="GetPostDataETag(string, long)"/>
        Task<PostData> GetPostData(string timelineName, long postId);

        /// <summary>
        /// Create a new text post in timeline.
        /// </summary>
        /// <param name="timelineName">The name of the timeline to create post against.</param>
        /// <param name="authorId">The author's user id.</param>
        /// <param name="text">The content text.</param>
        /// <param name="time">The time of the post. If null, then current time is used.</param>
        /// <returns>The info of the created post.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> or <paramref name="text"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="UserNotExistException">Thrown if user of <paramref name="authorId"/> does not exist.</exception>
        Task<TimelinePost> CreateTextPost(string timelineName, long authorId, string text, DateTime? time);

        /// <summary>
        /// Create a new image post in timeline.
        /// </summary>
        /// <param name="timelineName">The name of the timeline to create post against.</param>
        /// <param name="authorId">The author's user id.</param>
        /// <param name="imageData">The image data.</param>
        /// <param name="time">The time of the post. If null, then use current time.</param>
        /// <returns>The info of the created post.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> or <paramref name="imageData"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="UserNotExistException">Thrown if user of <paramref name="authorId"/> does not exist.</exception>
        /// <exception cref="ImageException">Thrown if data is not a image. Validated by <see cref="ImageValidator"/>.</exception>
        Task<TimelinePost> CreateImagePost(string timelineName, long authorId, byte[] imageData, DateTime? time);

        /// <summary>
        /// Delete a post.
        /// </summary>
        /// <param name="timelineName">The name of the timeline to delete post against.</param>
        /// <param name="postId">The id of the post to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="TimelinePostNotExistException">Thrown when the post with given id does not exist or is deleted already.</exception>
        /// <remarks>
        /// First use <see cref="HasPostModifyPermission(string, long, long, bool)"/> to check the permission.
        /// </remarks>
        Task DeletePost(string timelineName, long postId);

        /// <summary>
        /// Delete all posts of the given user. Used when delete a user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        Task DeleteAllPostsOfUser(long userId);

        /// <summary>
        /// Verify whether a user has the permission to modify a post.
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <param name="postId">The id of the post.</param>
        /// <param name="modifierId">The id of the user to check on.</param>
        /// <param name="throwOnPostNotExist">True if you want it to throw <see cref="TimelinePostNotExistException"/>. Default false.</param>
        /// <returns>True if can modify, false if can't modify.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="TimelinePostNotExistException">Thrown when the post with given id does not exist or is deleted already and <paramref name="throwOnPostNotExist"/> is true.</exception>
        /// <remarks>
        /// Unless <paramref name="throwOnPostNotExist"/> is true, this method should return true if the post does not exist.
        /// If the post is deleted, its author info still exists, so it is checked as the post is not deleted unless <paramref name="throwOnPostNotExist"/> is true.
        /// This method does not check whether the user is administrator.
        /// It only checks whether he is the author of the post or the owner of the timeline.
        /// Return false when user with modifier id does not exist.
        /// </remarks>
        Task<bool> HasPostModifyPermission(string timelineName, long postId, long modifierId, bool throwOnPostNotExist = false);
    }

    public class TimelinePostService : ITimelinePostService
    {
        private readonly ILogger<TimelinePostService> _logger;
        private readonly DatabaseContext _database;
        private readonly IBasicTimelineService _basicTimelineService;
        private readonly IUserService _userService;
        private readonly IDataManager _dataManager;
        private readonly IImageValidator _imageValidator;
        private readonly IClock _clock;

        public TimelinePostService(ILogger<TimelinePostService> logger, DatabaseContext database, IBasicTimelineService basicTimelineService, IUserService userService, IDataManager dataManager, IImageValidator imageValidator, IClock clock)
        {
            _logger = logger;
            _database = database;
            _basicTimelineService = basicTimelineService;
            _userService = userService;
            _dataManager = dataManager;
            _imageValidator = imageValidator;
            _clock = clock;
        }

        private async Task<TimelinePost> MapTimelinePostFromEntity(TimelinePostEntity entity, string timelineName)
        {
            User? author = entity.AuthorId.HasValue ? await _userService.GetUser(entity.AuthorId.Value) : null;

            ITimelinePostContent? content = null;

            if (entity.Content != null)
            {
                var type = entity.ContentType;

                content = type switch
                {
                    TimelinePostContentTypes.Text => new TextTimelinePostContent(entity.Content),
                    TimelinePostContentTypes.Image => new ImageTimelinePostContent(entity.Content),
                    _ => throw new DatabaseCorruptedException(string.Format(CultureInfo.InvariantCulture, ExceptionDatabaseUnknownContentType, type))
                };
            }

            return new TimelinePost(
                    id: entity.LocalId,
                    author: author,
                    content: content,
                    time: entity.Time,
                    lastUpdated: entity.LastUpdated,
                    timelineName: timelineName
                );
        }

        public async Task<List<TimelinePost>> GetPosts(string timelineName, DateTime? modifiedSince = null, bool includeDeleted = false)
        {
            modifiedSince = modifiedSince?.MyToUtc();

            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await _basicTimelineService.GetTimelineIdByName(timelineName);
            IQueryable<TimelinePostEntity> query = _database.TimelinePosts.Where(p => p.TimelineId == timelineId);

            if (!includeDeleted)
            {
                query = query.Where(p => p.Content != null);
            }

            if (modifiedSince.HasValue)
            {
                query = query.Include(p => p.Author).Where(p => p.LastUpdated >= modifiedSince || (p.Author != null && p.Author.UsernameChangeTime >= modifiedSince));
            }

            query = query.OrderBy(p => p.Time);

            var postEntities = await query.ToListAsync();

            var posts = new List<TimelinePost>();
            foreach (var entity in postEntities)
            {
                posts.Add(await MapTimelinePostFromEntity(entity, timelineName));
            }
            return posts;
        }

        public async Task<string> GetPostDataETag(string timelineName, long postId)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await _basicTimelineService.GetTimelineIdByName(timelineName);

            var postEntity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).SingleOrDefaultAsync();

            if (postEntity == null)
                throw new TimelinePostNotExistException(timelineName, postId, false);

            if (postEntity.Content == null)
                throw new TimelinePostNotExistException(timelineName, postId, true);

            if (postEntity.ContentType != TimelinePostContentTypes.Image)
                throw new TimelinePostNoDataException(ExceptionGetDataNonImagePost);

            var tag = postEntity.Content;

            return tag;
        }

        public async Task<PostData> GetPostData(string timelineName, long postId)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await _basicTimelineService.GetTimelineIdByName(timelineName);
            var postEntity = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == postId).SingleOrDefaultAsync();

            if (postEntity == null)
                throw new TimelinePostNotExistException(timelineName, postId, false);

            if (postEntity.Content == null)
                throw new TimelinePostNotExistException(timelineName, postId, true);

            if (postEntity.ContentType != TimelinePostContentTypes.Image)
                throw new TimelinePostNoDataException(ExceptionGetDataNonImagePost);

            var tag = postEntity.Content;

            byte[] data;

            try
            {
                data = await _dataManager.GetEntry(tag);
            }
            catch (InvalidOperationException e)
            {
                throw new DatabaseCorruptedException(ExceptionGetDataDataEntryNotExist, e);
            }

            if (postEntity.ExtraContent == null)
            {
                _logger.LogWarning(LogGetDataNoFormat);
                var format = Image.DetectFormat(data);
                postEntity.ExtraContent = format.DefaultMimeType;
                await _database.SaveChangesAsync();
            }

            return new PostData
            {
                Data = data,
                Type = postEntity.ExtraContent,
                ETag = tag,
                LastModified = postEntity.LastUpdated
            };
        }

        public async Task<TimelinePost> CreateTextPost(string timelineName, long authorId, string text, DateTime? time)
        {
            time = time?.MyToUtc();

            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var timelineId = await _basicTimelineService.GetTimelineIdByName(timelineName);
            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).SingleAsync();

            var author = await _userService.GetUser(authorId);

            var currentTime = _clock.GetCurrentTime();
            var finalTime = time ?? currentTime;

            timelineEntity.CurrentPostLocalId += 1;

            var postEntity = new TimelinePostEntity
            {
                LocalId = timelineEntity.CurrentPostLocalId,
                ContentType = TimelinePostContentTypes.Text,
                Content = text,
                AuthorId = authorId,
                TimelineId = timelineId,
                Time = finalTime,
                LastUpdated = currentTime
            };
            _database.TimelinePosts.Add(postEntity);
            await _database.SaveChangesAsync();


            return new TimelinePost(
                id: postEntity.LocalId,
                content: new TextTimelinePostContent(text),
                time: finalTime,
                author: author,
                lastUpdated: currentTime,
                timelineName: timelineName
            );
        }

        public async Task<TimelinePost> CreateImagePost(string timelineName, long authorId, byte[] data, DateTime? time)
        {
            time = time?.MyToUtc();

            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var timelineId = await _basicTimelineService.GetTimelineIdByName(timelineName);
            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).SingleAsync();

            var author = await _userService.GetUser(authorId);

            var imageFormat = await _imageValidator.Validate(data);

            var imageFormatText = imageFormat.DefaultMimeType;

            var tag = await _dataManager.RetainEntry(data);

            var currentTime = _clock.GetCurrentTime();
            var finalTime = time ?? currentTime;

            timelineEntity.CurrentPostLocalId += 1;

            var postEntity = new TimelinePostEntity
            {
                LocalId = timelineEntity.CurrentPostLocalId,
                ContentType = TimelinePostContentTypes.Image,
                Content = tag,
                ExtraContent = imageFormatText,
                AuthorId = authorId,
                TimelineId = timelineId,
                Time = finalTime,
                LastUpdated = currentTime
            };
            _database.TimelinePosts.Add(postEntity);
            await _database.SaveChangesAsync();

            return new TimelinePost(
                id: postEntity.LocalId,
                content: new ImageTimelinePostContent(tag),
                time: finalTime,
                author: author,
                lastUpdated: currentTime,
                timelineName: timelineName
            );
        }

        public async Task DeletePost(string timelineName, long id)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await _basicTimelineService.GetTimelineIdByName(timelineName);

            var post = await _database.TimelinePosts.Where(p => p.TimelineId == timelineId && p.LocalId == id).SingleOrDefaultAsync();

            if (post == null)
                throw new TimelinePostNotExistException(timelineName, id, false);

            if (post.Content == null)
                throw new TimelinePostNotExistException(timelineName, id, true);

            string? dataTag = null;

            if (post.ContentType == TimelinePostContentTypes.Image)
            {
                dataTag = post.Content;
            }

            post.Content = null;
            post.LastUpdated = _clock.GetCurrentTime();

            await _database.SaveChangesAsync();

            if (dataTag != null)
            {
                await _dataManager.FreeEntry(dataTag);
            }
        }

        public async Task DeleteAllPostsOfUser(long userId)
        {
            var posts = await _database.TimelinePosts.Where(p => p.AuthorId == userId).ToListAsync();

            var now = _clock.GetCurrentTime();

            var dataTags = new List<string>();

            foreach (var post in posts)
            {
                if (post.Content != null)
                {
                    if (post.ContentType == TimelinePostContentTypes.Image)
                    {
                        dataTags.Add(post.Content);
                    }
                    post.Content = null;
                }
                post.LastUpdated = now;
            }

            await _database.SaveChangesAsync();

            foreach (var dataTag in dataTags)
            {
                await _dataManager.FreeEntry(dataTag);
            }
        }

        public async Task<bool> HasPostModifyPermission(string timelineName, long postId, long modifierId, bool throwOnPostNotExist = false)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await _basicTimelineService.GetTimelineIdByName(timelineName);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleAsync();

            var postEntity = await _database.TimelinePosts.Where(p => p.Id == postId).Select(p => new { p.Content, p.AuthorId }).SingleOrDefaultAsync();

            if (postEntity == null)
            {
                if (throwOnPostNotExist)
                    throw new TimelinePostNotExistException(timelineName, postId, false);
                else
                    return true;
            }

            if (postEntity.Content == null && throwOnPostNotExist)
            {
                throw new TimelinePostNotExistException(timelineName, postId, true);
            }

            return timelineEntity.OwnerId == modifierId || postEntity.AuthorId == modifierId;
        }
    }
}
