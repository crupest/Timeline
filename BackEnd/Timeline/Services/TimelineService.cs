using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers;
using Timeline.Models;
using Timeline.Models.Validation;
using Timeline.Services.Exceptions;
using static Timeline.Resources.Services.TimelineService;

namespace Timeline.Services
{
    public static class TimelineHelper
    {
        public static string ExtractTimelineName(string name, out bool isPersonal)
        {
            if (name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
            {
                isPersonal = true;
                return name.Substring(1);
            }
            else
            {
                isPersonal = false;
                return name;
            }
        }
    }

    public enum TimelineUserRelationshipType
    {
        Own = 0b1,
        Join = 0b10,
        Default = Own | Join
    }

    public class TimelineUserRelationship
    {
        public TimelineUserRelationship(TimelineUserRelationshipType type, long userId)
        {
            Type = type;
            UserId = userId;
        }

        public TimelineUserRelationshipType Type { get; set; }
        public long UserId { get; set; }
    }

    public class PostData : ICacheableData
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; set; } = default!;
#pragma warning restore CA1819 // Properties should not return arrays
        public string Type { get; set; } = default!;
        public string ETag { get; set; } = default!;
        public DateTime? LastModified { get; set; } // TODO: Why nullable?
    }

    /// <summary>
    /// This define the interface of both personal timeline and ordinary timeline.
    /// </summary>
    public interface ITimelineService
    {
        /// <summary>
        /// Get the timeline last modified time (not include name change).
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <returns>The timeline info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        Task<DateTime> GetTimelineLastModifiedTime(string timelineName);

        /// <summary>
        /// Get the timeline unique id.
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <returns>The timeline info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        Task<string> GetTimelineUniqueId(string timelineName);

        /// <summary>
        /// Get the timeline info.
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <returns>The timeline info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        Task<Models.Timeline> GetTimeline(string timelineName);

        /// <summary>
        /// Set the properties of a timeline. 
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <param name="newProperties">The new properties. Null member means not to change.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> or <paramref name="newProperties"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        Task ChangeProperty(string timelineName, TimelineChangePropertyRequest newProperties);

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
        /// Change member of timeline.
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <param name="membersToAdd">A list of usernames of members to add. May be null.</param>
        /// <param name="membersToRemove">A list of usernames of members to remove. May be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when names in <paramref name="membersToAdd"/> or <paramref name="membersToRemove"/> is not a valid username.</exception>
        /// <exception cref="UserNotExistException">Thrown when one of the user to change does not exist.</exception>
        /// <remarks>
        /// Operating on a username that is of bad format or does not exist always throws.
        /// Add a user that already is a member has no effects.
        /// Remove a user that is not a member also has not effects.
        /// Add and remove an identical user results in no effects.
        /// More than one same usernames are regarded as one.
        /// </remarks>
        Task ChangeMember(string timelineName, IList<string>? membersToAdd, IList<string>? membersToRemove);

        /// <summary>
        /// Check whether a user can manage(change timeline info, member, ...) a timeline.
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <param name="userId">The id of the user to check on.</param>
        /// <returns>True if the user can manage the timeline, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <remarks>
        /// This method does not check whether visitor is administrator.
        /// Return false if user with user id does not exist.
        /// </remarks>
        Task<bool> HasManagePermission(string timelineName, long userId);

        /// <summary>
        /// Verify whether a visitor has the permission to read a timeline.
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <param name="visitorId">The id of the user to check on. Null means visitor without account.</param>
        /// <returns>True if can read, false if can't read.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <remarks>
        /// This method does not check whether visitor is administrator.
        /// Return false if user with visitor id does not exist.
        /// </remarks>
        Task<bool> HasReadPermission(string timelineName, long? visitorId);

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

        /// <summary>
        /// Verify whether a user is member of a timeline.
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <param name="userId">The id of user to check on.</param>
        /// <returns>True if it is a member, false if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <remarks>
        /// Timeline owner is also considered as a member.
        /// Return false when user with user id does not exist.
        /// </remarks>
        Task<bool> IsMemberOf(string timelineName, long userId);

        /// <summary>
        /// Get all timelines including personal and ordinary timelines.
        /// </summary>
        /// <param name="relate">Filter timelines related (own or is a member) to specific user.</param>
        /// <param name="visibility">Filter timelines with given visibility. If null or empty, all visibilities are returned. Duplicate value are ignored.</param>
        /// <returns>The list of timelines.</returns>
        /// <remarks>
        /// If user with related user id does not exist, empty list will be returned.
        /// </remarks>
        Task<List<Models.Timeline>> GetTimelines(TimelineUserRelationship? relate = null, List<TimelineVisibility>? visibility = null);

        /// <summary>
        /// Create a timeline.
        /// </summary>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <param name="ownerId">The id of owner of the timeline.</param>
        /// <returns>The info of the new timeline.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when timeline name is invalid.</exception>
        /// <exception cref="EntityAlreadyExistException">Thrown when the timeline already exists.</exception>
        /// <exception cref="UserNotExistException">Thrown when the owner user does not exist.</exception>
        Task<Models.Timeline> CreateTimeline(string timelineName, long ownerId);

        /// <summary>
        /// Delete a timeline.
        /// </summary>
        /// <param name="timelineName">The name of the timeline to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when timeline name is invalid.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when the timeline does not exist.</exception>
        Task DeleteTimeline(string timelineName);

        /// <summary>
        /// Change name of a timeline.
        /// </summary>
        /// <param name="oldTimelineName">The old timeline name.</param>
        /// <param name="newTimelineName">The new timeline name.</param>
        /// <returns>The new timeline info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="oldTimelineName"/> or <paramref name="newTimelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="oldTimelineName"/> or <paramref name="newTimelineName"/> is of invalid format.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="EntityAlreadyExistException">Thrown when a timeline with new name already exists.</exception>
        /// <remarks>
        /// You can only change name of general timeline.
        /// </remarks>
        Task<Models.Timeline> ChangeTimelineName(string oldTimelineName, string newTimelineName);
    }

    public class TimelineService : ITimelineService
    {
        public TimelineService(ILogger<TimelineService> logger, DatabaseContext database, IDataManager dataManager, IUserService userService, IImageValidator imageValidator, IClock clock)
        {
            _logger = logger;
            _database = database;
            _dataManager = dataManager;
            _userService = userService;
            _imageValidator = imageValidator;
            _clock = clock;
        }

        private readonly ILogger<TimelineService> _logger;

        private readonly DatabaseContext _database;

        private readonly IDataManager _dataManager;

        private readonly IUserService _userService;

        private readonly IImageValidator _imageValidator;

        private readonly IClock _clock;

        private readonly UsernameValidator _usernameValidator = new UsernameValidator();

        private readonly TimelineNameValidator _timelineNameValidator = new TimelineNameValidator();

        private void ValidateTimelineName(string name, string paramName)
        {
            if (!_timelineNameValidator.Validate(name, out var message))
            {
                throw new ArgumentException(ExceptionTimelineNameBadFormat.AppendAdditionalMessage(message), paramName);
            }
        }

        /// Remember to include Members when query.
        private async Task<Models.Timeline> MapTimelineFromEntity(TimelineEntity entity)
        {
            var owner = await _userService.GetUser(entity.OwnerId);

            var members = new List<User>();
            foreach (var memberEntity in entity.Members)
            {
                members.Add(await _userService.GetUser(memberEntity.UserId));
            }

            var name = entity.Name ?? ("@" + owner.Username);

            return new Models.Timeline
            {
                UniqueID = entity.UniqueId,
                Name = name,
                NameLastModified = entity.NameLastModified,
                Title = string.IsNullOrEmpty(entity.Title) ? name : entity.Title,
                Description = entity.Description ?? "",
                Owner = owner,
                Visibility = entity.Visibility,
                Members = members,
                CreateTime = entity.CreateTime,
                LastModified = entity.LastModified
            };
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

        private TimelineEntity CreateNewTimelineEntity(string? name, long ownerId)
        {
            var currentTime = _clock.GetCurrentTime();

            return new TimelineEntity
            {
                Name = name,
                NameLastModified = currentTime,
                OwnerId = ownerId,
                Visibility = TimelineVisibility.Register,
                CreateTime = currentTime,
                LastModified = currentTime,
                CurrentPostLocalId = 0,
                Members = new List<TimelineMemberEntity>()
            };
        }



        // Get timeline id by name. If it is a personal timeline and it does not exist, it will be created.
        //
        // This method will check the name format and if it is invalid, ArgumentException is thrown.
        //
        // For personal timeline, if the user does not exist, TimelineNotExistException will be thrown with UserNotExistException as inner exception.
        // For ordinary timeline, if the timeline does not exist, TimelineNotExistException will be thrown.
        //
        // It follows all timeline-related function common interface contracts.
        private async Task<long> FindTimelineId(string timelineName)
        {
            timelineName = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);

            if (isPersonal)
            {
                long userId;
                try
                {
                    userId = await _userService.GetUserIdByUsername(timelineName);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException(ExceptionFindTimelineUsernameBadFormat, nameof(timelineName), e);
                }
                catch (UserNotExistException e)
                {
                    throw new TimelineNotExistException(timelineName, e);
                }

                var timelineEntity = await _database.Timelines.Where(t => t.OwnerId == userId && t.Name == null).Select(t => new { t.Id }).SingleOrDefaultAsync();

                if (timelineEntity != null)
                {
                    return timelineEntity.Id;
                }
                else
                {
                    var newTimelineEntity = CreateNewTimelineEntity(null, userId);
                    _database.Timelines.Add(newTimelineEntity);
                    await _database.SaveChangesAsync();

                    return newTimelineEntity.Id;
                }
            }
            else
            {
                if (timelineName == null)
                    throw new ArgumentNullException(nameof(timelineName));

                ValidateTimelineName(timelineName, nameof(timelineName));

                var timelineEntity = await _database.Timelines.Where(t => t.Name == timelineName).Select(t => new { t.Id }).SingleOrDefaultAsync();

                if (timelineEntity == null)
                {
                    throw new TimelineNotExistException(timelineName);
                }
                else
                {
                    return timelineEntity.Id;
                }
            }
        }

        public async Task<DateTime> GetTimelineLastModifiedTime(string timelineName)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await FindTimelineId(timelineName);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.LastModified }).SingleAsync();

            return timelineEntity.LastModified;
        }

        public async Task<string> GetTimelineUniqueId(string timelineName)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await FindTimelineId(timelineName);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.UniqueId }).SingleAsync();

            return timelineEntity.UniqueId;
        }

        public async Task<Models.Timeline> GetTimeline(string timelineName)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await FindTimelineId(timelineName);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Include(t => t.Members).SingleAsync();

            return await MapTimelineFromEntity(timelineEntity);
        }

        public async Task<List<TimelinePost>> GetPosts(string timelineName, DateTime? modifiedSince = null, bool includeDeleted = false)
        {
            modifiedSince = modifiedSince?.MyToUtc();

            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await FindTimelineId(timelineName);
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

            var timelineId = await FindTimelineId(timelineName);

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

            var timelineId = await FindTimelineId(timelineName);
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

            var timelineId = await FindTimelineId(timelineName);
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

            var timelineId = await FindTimelineId(timelineName);
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

            var timelineId = await FindTimelineId(timelineName);

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

        public async Task ChangeProperty(string timelineName, TimelineChangePropertyRequest newProperties)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));
            if (newProperties == null)
                throw new ArgumentNullException(nameof(newProperties));

            var timelineId = await FindTimelineId(timelineName);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).SingleAsync();

            var changed = false;

            if (newProperties.Title != null)
            {
                changed = true;
                timelineEntity.Title = newProperties.Title;
            }

            if (newProperties.Description != null)
            {
                changed = true;
                timelineEntity.Description = newProperties.Description;
            }

            if (newProperties.Visibility.HasValue)
            {
                changed = true;
                timelineEntity.Visibility = newProperties.Visibility.Value;
            }

            if (changed)
            {
                var currentTime = _clock.GetCurrentTime();
                timelineEntity.LastModified = currentTime;
            }

            await _database.SaveChangesAsync();
        }

        public async Task ChangeMember(string timelineName, IList<string>? add, IList<string>? remove)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            List<string>? RemoveDuplicateAndCheckFormat(IList<string>? list, string paramName)
            {
                if (list != null)
                {
                    List<string> result = new List<string>();
                    var count = list.Count;
                    for (var index = 0; index < count; index++)
                    {
                        var username = list[index];
                        if (result.Contains(username))
                        {
                            continue;
                        }
                        var (validationResult, message) = _usernameValidator.Validate(username);
                        if (!validationResult)
                            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionChangeMemberUsernameBadFormat, index), nameof(paramName));
                        result.Add(username);
                    }
                    return result;
                }
                else
                {
                    return null;
                }
            }
            var simplifiedAdd = RemoveDuplicateAndCheckFormat(add, nameof(add));
            var simplifiedRemove = RemoveDuplicateAndCheckFormat(remove, nameof(remove));

            // remove those both in add and remove
            if (simplifiedAdd != null && simplifiedRemove != null)
            {
                var usersToClean = simplifiedRemove.Where(u => simplifiedAdd.Contains(u)).ToList();
                foreach (var u in usersToClean)
                {
                    simplifiedAdd.Remove(u);
                    simplifiedRemove.Remove(u);
                }

                if (simplifiedAdd.Count == 0)
                    simplifiedAdd = null;

                if (simplifiedRemove.Count == 0)
                    simplifiedRemove = null;
            }

            if (simplifiedAdd == null && simplifiedRemove == null)
                return;

            var timelineId = await FindTimelineId(timelineName);

            async Task<List<long>?> CheckExistenceAndGetId(List<string>? list)
            {
                if (list == null)
                    return null;

                List<long> result = new List<long>();
                foreach (var username in list)
                {
                    result.Add(await _userService.GetUserIdByUsername(username));
                }
                return result;
            }
            var userIdsAdd = await CheckExistenceAndGetId(simplifiedAdd);
            var userIdsRemove = await CheckExistenceAndGetId(simplifiedRemove);

            if (userIdsAdd != null)
            {
                var membersToAdd = userIdsAdd.Select(id => new TimelineMemberEntity { UserId = id, TimelineId = timelineId }).ToList();
                _database.TimelineMembers.AddRange(membersToAdd);
            }

            if (userIdsRemove != null)
            {
                var membersToRemove = await _database.TimelineMembers.Where(m => m.TimelineId == timelineId && userIdsRemove.Contains(m.UserId)).ToListAsync();
                _database.TimelineMembers.RemoveRange(membersToRemove);
            }

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).SingleAsync();
            timelineEntity.LastModified = _clock.GetCurrentTime();

            await _database.SaveChangesAsync();
        }

        public async Task<bool> HasManagePermission(string timelineName, long userId)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await FindTimelineId(timelineName);
            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleAsync();

            return userId == timelineEntity.OwnerId;
        }

        public async Task<bool> HasReadPermission(string timelineName, long? visitorId)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await FindTimelineId(timelineName);
            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.Visibility }).SingleAsync();

            if (timelineEntity.Visibility == TimelineVisibility.Public)
                return true;

            if (timelineEntity.Visibility == TimelineVisibility.Register && visitorId != null)
                return true;

            if (visitorId == null)
            {
                return false;
            }
            else
            {
                var memberEntity = await _database.TimelineMembers.Where(m => m.UserId == visitorId && m.TimelineId == timelineId).SingleOrDefaultAsync();
                return memberEntity != null;
            }
        }

        public async Task<bool> HasPostModifyPermission(string timelineName, long postId, long modifierId, bool throwOnPostNotExist = false)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await FindTimelineId(timelineName);

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

        public async Task<bool> IsMemberOf(string timelineName, long userId)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await FindTimelineId(timelineName);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleAsync();

            if (userId == timelineEntity.OwnerId)
                return true;

            return await _database.TimelineMembers.AnyAsync(m => m.TimelineId == timelineId && m.UserId == userId);
        }

        public async Task<List<Models.Timeline>> GetTimelines(TimelineUserRelationship? relate = null, List<TimelineVisibility>? visibility = null)
        {
            List<TimelineEntity> entities;

            IQueryable<TimelineEntity> ApplyTimelineVisibilityFilter(IQueryable<TimelineEntity> query)
            {
                if (visibility != null && visibility.Count != 0)
                {
                    return query.Where(t => visibility.Contains(t.Visibility));
                }
                return query;
            }

            bool allVisibilities = visibility == null || visibility.Count == 0;

            if (relate == null)
            {
                entities = await ApplyTimelineVisibilityFilter(_database.Timelines).Include(t => t.Members).ToListAsync();
            }
            else
            {
                entities = new List<TimelineEntity>();

                if ((relate.Type & TimelineUserRelationshipType.Own) != 0)
                {
                    entities.AddRange(await ApplyTimelineVisibilityFilter(_database.Timelines.Where(t => t.OwnerId == relate.UserId)).Include(t => t.Members).ToListAsync());
                }

                if ((relate.Type & TimelineUserRelationshipType.Join) != 0)
                {
                    entities.AddRange(await ApplyTimelineVisibilityFilter(_database.TimelineMembers.Where(m => m.UserId == relate.UserId).Include(m => m.Timeline).ThenInclude(t => t.Members).Select(m => m.Timeline)).ToListAsync());
                }
            }

            var result = new List<Models.Timeline>();

            foreach (var entity in entities)
            {
                result.Add(await MapTimelineFromEntity(entity));
            }

            return result;
        }

        public async Task<Models.Timeline> CreateTimeline(string name, long owner)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            ValidateTimelineName(name, nameof(name));

            var user = await _userService.GetUser(owner);

            var conflict = await _database.Timelines.AnyAsync(t => t.Name == name);

            if (conflict)
                throw new EntityAlreadyExistException(EntityNames.Timeline, null, ExceptionTimelineNameConflict);

            var newEntity = CreateNewTimelineEntity(name, user.Id);

            _database.Timelines.Add(newEntity);
            await _database.SaveChangesAsync();

            return await MapTimelineFromEntity(newEntity);
        }

        public async Task DeleteTimeline(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            ValidateTimelineName(name, nameof(name));

            var entity = await _database.Timelines.Where(t => t.Name == name).SingleOrDefaultAsync();

            if (entity == null)
                throw new TimelineNotExistException(name);

            _database.Timelines.Remove(entity);
            await _database.SaveChangesAsync();
        }

        public async Task<Models.Timeline> ChangeTimelineName(string oldTimelineName, string newTimelineName)
        {
            if (oldTimelineName == null)
                throw new ArgumentNullException(nameof(oldTimelineName));
            if (newTimelineName == null)
                throw new ArgumentNullException(nameof(newTimelineName));

            ValidateTimelineName(oldTimelineName, nameof(oldTimelineName));
            ValidateTimelineName(newTimelineName, nameof(newTimelineName));

            var entity = await _database.Timelines.Include(t => t.Members).Where(t => t.Name == oldTimelineName).SingleOrDefaultAsync();

            if (entity == null)
                throw new TimelineNotExistException(oldTimelineName);

            if (oldTimelineName == newTimelineName)
                return await MapTimelineFromEntity(entity);

            var conflict = await _database.Timelines.AnyAsync(t => t.Name == newTimelineName);

            if (conflict)
                throw new EntityAlreadyExistException(EntityNames.Timeline, null, ExceptionTimelineNameConflict);

            var now = _clock.GetCurrentTime();

            entity.Name = newTimelineName;
            entity.NameLastModified = now;
            entity.LastModified = now;

            await _database.SaveChangesAsync();

            return await MapTimelineFromEntity(entity);
        }
    }
}
