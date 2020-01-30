using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Models.Validation;
using static Timeline.Resources.Services.TimelineService;

namespace Timeline.Services
{
    /// <summary>
    /// This define the common interface of both personal timeline
    /// and normal timeline.
    /// </summary>
    /// <remarks>
    /// The "name" parameter in method means name of timeline in
    /// <see cref="ITimelineService"/> while username of the owner
    /// of the personal timeline in <see cref="IPersonalTimelineService"/>.
    /// </remarks>
    public interface IBaseTimelineService
    {
        /// <summary>
        /// Get all the posts in the timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <returns>A list of all posts.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is illegal. It is not a valid timeline name (for normal timeline service) or a valid username (for personal timeline service).</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        Task<List<TimelinePostInfo>> GetPosts(string name);

        /// <summary>
        /// Create a new post in timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <ssee cref="IBaseTimelineService"/>.</param>
        /// <param name="authorId">The author's id.</param>
        /// <param name="content">The content.</param>
        /// <param name="time">The time of the post. If null, then use current time.</param>
        /// <returns>The info of the created post.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="content"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is illegal. It is not a valid timeline name (for normal timeline service) or a valid username (for personal timeline service).</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="UserNotExistException">Thrown if user with <paramref name="authorId"/> does not exist.</exception>
        Task<TimelinePostInfo> CreatePost(string name, long authorId, string content, DateTime? time);

        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="id">The id of the post to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="username"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is illegal. It is not a valid timeline name (for normal timeline service) or a valid username (for personal timeline service).</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="TimelinePostNotExistException">
        /// Thrown when the post with given id does not exist or is deleted already.
        /// </exception>
        /// <remarks>
        /// First use <see cref="IBaseTimelineService.HasPostModifyPermission(string, long, string)"/>
        /// to check the permission.
        /// </remarks>
        Task DeletePost(string name, long id);

        /// <summary>
        /// Set the properties of a timeline. 
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="newProperties">The new properties. Null member means not to change.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="newProperties"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is illegal. It is not a valid timeline name (for normal timeline service) or a valid username (for personal timeline service).</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        Task ChangeProperty(string name, TimelinePatchRequest newProperties);

        /// <summary>
        /// Remove members to a timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="add">A list of usernames of members to add. May be null.</param>
        /// <param name="remove">A list of usernames of members to remove. May be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is illegal. It is not a valid timeline name (for normal timeline service) or a valid username (for personal timeline service).</exception>
        /// <exception cref="ArgumentException">Thrown when names in <paramref name="add"/> or <paramref name="remove"/> is not a valid username.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="UserNotExistException">
        /// Thrown when one of the user to change does not exist.
        /// </exception>
        /// <remarks>
        /// Operating on a username that is of bad format or does not exist always throws.
        /// Add a user that already is a member has no effects.
        /// Remove a user that is not a member also has not effects.
        /// Add and remove an identical user results in no effects.
        /// More than one same usernames are regarded as one.
        /// </remarks>
        Task ChangeMember(string name, IList<string>? add, IList<string>? remove);

        /// <summary>
        /// Verify whether a visitor has the permission to read a timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="visitorId">The id of the user to check on. Null means visitor without account.</param>
        /// <returns>True if can read, false if can't read.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is illegal. It is not a valid timeline name (for normal timeline service) or a valid username (for personal timeline service).</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <remarks>
        /// This method does not check whether visitor is administrator.
        /// Return false if user with visitor id does not exist.
        /// </remarks>
        Task<bool> HasReadPermission(string name, long? visitorId);

        /// <summary>
        /// Verify whether a user has the permission to modify a post.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="modifierId">The id of the user to check on.</param>
        /// <returns>True if can modify, false if can't modify.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is illegal. It is not a valid timeline name (for normal timeline service) or a valid username (for personal timeline service).</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="TimelinePostNotExistException">
        /// Thrown when the post with given id does not exist or is deleted already.
        /// </exception>
        /// <remarks>
        /// This method does not check whether the user is administrator.
        /// It only checks whether he is the author of the post or the owner of the timeline.
        /// Return false when user with modifier id does not exist.
        /// </remarks>
        Task<bool> HasPostModifyPermission(string name, long id, long modifierId);

        /// <summary>
        /// Verify whether a user is member of a timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="userId">The id of user to check on.</param>
        /// <returns>True if it is a member, false if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is illegal. It is not a valid timeline name (for normal timeline service) or a valid username (for personal timeline service).</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <remarks>
        /// Timeline owner is also considered as a member.
        /// Return false when user with user id does not exist.
        /// </remarks>
        Task<bool> IsMemberOf(string name, long userId);
    }

    /// <summary>
    /// Service for normal timeline.
    /// </summary>
    public interface ITimelineService : IBaseTimelineService
    {
        /// <summary>
        /// Get the timeline info.
        /// </summary>
        /// <param name="name">The name of the timeline.</param>
        /// <returns>The timeline info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when timeline name is invalid. Currently it means it is an empty string.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with the name does not exist.
        /// </exception>
        Task<TimelineInfo> GetTimeline(string name);

        /// <summary>
        /// Create a timeline.
        /// </summary>
        /// <param name="name">The name of the timeline.</param>
        /// <param name="owner">The id of owner of the timeline.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="owner"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when timeline name is invalid. Currently it means it is an empty string.</exception>
        /// <exception cref="ConflictException">Thrown when the timeline already exists.</exception>
        /// <exception cref="UserNotExistException">Thrown when the owner user does not exist.</exception>
        Task CreateTimeline(string name, long owner);
    }

    public interface IPersonalTimelineService : IBaseTimelineService
    {
        /// <summary>
        /// Get the timeline info.
        /// </summary>
        /// <param name="username">The username of the owner of the personal timeline.</param>
        /// <returns>The timeline info.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="username"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="username"/> is of bad format.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when the user does not exist. Inner exception MUST be <see cref="UserNotExistException"/>.
        /// </exception>
        Task<BaseTimelineInfo> GetTimeline(string username);
    }

    public abstract class BaseTimelineService : IBaseTimelineService
    {
        protected BaseTimelineService(ILoggerFactory loggerFactory, DatabaseContext database, IUserService userService, IMapper mapper, IClock clock)
        {
            Clock = clock;
            Database = database;
            UserService = userService;
            Mapper = mapper;
        }

        protected IClock Clock { get; }

        protected UsernameValidator UsernameValidator { get; } = new UsernameValidator();

        protected DatabaseContext Database { get; }

        protected IUserService UserService { get; }

        protected IMapper Mapper { get; }

        /// <summary>
        /// Find the timeline id by the name.
        /// For details, see remarks.
        /// </summary>
        /// <param name="name">The username or the timeline name. See remarks.</param>
        /// <returns>The id of the timeline entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is illegal. It is not a valid timeline name (for normal timeline service) or a valid username (for personal timeline service).</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <remarks>
        /// This is the common but different part for both types of timeline service.
        /// For class that implements <see cref="IPersonalTimelineService"/>, this method should
        /// find the timeline entity id by the given <paramref name="name"/> as the username of the owner.
        /// For class that implements <see cref="ITimelineService"/>, this method should
        /// find the timeline entity id by the given <paramref name="name"/> as the timeline name.
        /// This method should be called by many other method that follows the contract.
        /// </remarks>
        protected abstract Task<long> FindTimelineId(string name);

        public async Task<List<TimelinePostInfo>> GetPosts(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));


            var timelineId = await FindTimelineId(name);
            var postEntities = await Database.TimelinePosts.OrderBy(p => p.Time).Where(p => p.TimelineId == timelineId && p.Content != null).ToListAsync();

            var posts = new List<TimelinePostInfo>();
            foreach (var entity in postEntities)
            {
                if (entity.Content != null) // otherwise it is deleted
                {
                    var author = Mapper.Map<UserInfo>(await UserService.GetUserById(entity.AuthorId));
                    posts.Add(new TimelinePostInfo
                    {
                        Id = entity.Id,
                        Content = entity.Content,
                        Author = author,
                        Time = entity.Time,
                        LastUpdated = entity.LastUpdated
                    });
                }
            }
            return posts;
        }

        public async Task<TimelinePostInfo> CreatePost(string name, long authorId, string content, DateTime? time)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var timelineId = await FindTimelineId(name);
            var author = Mapper.Map<UserInfo>(await UserService.GetUserById(authorId));

            var currentTime = Clock.GetCurrentTime();
            var finalTime = time ?? currentTime;

            var postEntity = new TimelinePostEntity
            {
                Content = content,
                AuthorId = authorId,
                TimelineId = timelineId,
                Time = finalTime,
                LastUpdated = currentTime
            };
            Database.TimelinePosts.Add(postEntity);
            await Database.SaveChangesAsync();

            return new TimelinePostInfo
            {
                Id = postEntity.Id,
                Content = content,
                Author = author,
                Time = finalTime,
                LastUpdated = currentTime
            };
        }

        public async Task DeletePost(string name, long id)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var post = await Database.TimelinePosts.Where(p => p.Id == id).SingleOrDefaultAsync();

            if (post == null)
                throw new TimelinePostNotExistException(id);

            post.Content = null;
            post.LastUpdated = Clock.GetCurrentTime();

            await Database.SaveChangesAsync();
        }

        public async Task ChangeProperty(string name, TimelinePatchRequest newProperties)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (newProperties == null)
                throw new ArgumentNullException(nameof(newProperties));

            var timelineId = await FindTimelineId(name);

            var timelineEntity = await Database.Timelines.Where(t => t.Id == timelineId).SingleAsync();

            if (newProperties.Description != null)
            {
                timelineEntity.Description = newProperties.Description;
            }

            if (newProperties.Visibility.HasValue)
            {
                timelineEntity.Visibility = newProperties.Visibility.Value;
            }

            await Database.SaveChangesAsync();
        }

        public async Task ChangeMember(string name, IList<string>? add, IList<string>? remove)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

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
                        var (validationResult, message) = UsernameValidator.Validate(username);
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
            }

            var timelineId = await FindTimelineId(name);

            async Task<List<long>?> CheckExistenceAndGetId(List<string>? list)
            {
                if (list == null)
                    return null;

                List<long> result = new List<long>();
                foreach (var username in list)
                {
                    result.Add(await UserService.GetUserIdByUsername(username));
                }
                return result;
            }
            var userIdsAdd = await CheckExistenceAndGetId(simplifiedAdd);
            var userIdsRemove = await CheckExistenceAndGetId(simplifiedRemove);

            if (userIdsAdd != null)
            {
                var membersToAdd = userIdsAdd.Select(id => new TimelineMemberEntity { UserId = id, TimelineId = timelineId }).ToList();
                Database.TimelineMembers.AddRange(membersToAdd);
            }

            if (userIdsRemove != null)
            {
                var membersToRemove = await Database.TimelineMembers.Where(m => m.TimelineId == timelineId && userIdsRemove.Contains(m.UserId)).ToListAsync();
                Database.TimelineMembers.RemoveRange(membersToRemove);
            }

            await Database.SaveChangesAsync();
        }

        public async Task<bool> HasReadPermission(string name, long? visitorId)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var timelineId = await FindTimelineId(name);

            var timelineEntity = await Database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.Visibility }).SingleAsync();

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
                var memberEntity = await Database.TimelineMembers.Where(m => m.UserId == visitorId && m.TimelineId == timelineId).SingleOrDefaultAsync();
                return memberEntity != null;
            }
        }

        public async Task<bool> HasPostModifyPermission(string name, long id, long modifierId)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var timelineId = await FindTimelineId(name);

            var timelineEntity = await Database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleAsync();

            var postEntity = await Database.TimelinePosts.Where(p => p.Id == id).Select(p => new { p.AuthorId }).SingleOrDefaultAsync();

            if (postEntity == null)
                throw new TimelinePostNotExistException(id);

            return timelineEntity.OwnerId == modifierId || postEntity.AuthorId == modifierId;
        }

        public async Task<bool> IsMemberOf(string name, long userId)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var timelineId = await FindTimelineId(name);

            var timelineEntity = await Database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleAsync();

            if (userId == timelineEntity.OwnerId)
                return true;

            return await Database.TimelineMembers.AnyAsync(m => m.TimelineId == timelineId && m.UserId == userId);
        }
    }

    public class PersonalTimelineService : BaseTimelineService, IPersonalTimelineService
    {
        public PersonalTimelineService(ILoggerFactory loggerFactory, DatabaseContext database, IUserService userService, IMapper mapper, IClock clock)
            : base(loggerFactory, database, userService, mapper, clock)
        {

        }

        protected override async Task<long> FindTimelineId(string name)
        {
            long userId;
            try
            {
                userId = await UserService.GetUserIdByUsername(name);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(ExceptionFindTimelineUsernameBadFormat, nameof(name), e);
            }
            catch (UserNotExistException e)
            {
                throw new TimelineNotExistException(name, e);
            }

            var timelineEntity = await Database.Timelines.Where(t => t.OwnerId == userId && t.Name == null).Select(t => new { t.Id }).SingleOrDefaultAsync();

            if (timelineEntity != null)
            {
                return timelineEntity.Id;
            }
            else
            {
                var newTimelineEntity = new TimelineEntity
                {
                    Name = null,
                    Description = null,
                    OwnerId = userId,
                    Visibility = TimelineVisibility.Register,
                    CreateTime = Clock.GetCurrentTime(),
                };
                Database.Timelines.Add(newTimelineEntity);
                await Database.SaveChangesAsync();

                return newTimelineEntity.Id;
            }
        }

        public async Task<BaseTimelineInfo> GetTimeline(string username)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            var timelineId = await FindTimelineId(username);

            var timelineEntity = await Database.Timelines.Where(t => t.Id == timelineId).SingleAsync();

            var timelineMemberEntities = await Database.TimelineMembers.Where(m => m.TimelineId == timelineId).Select(m => new { m.UserId }).ToListAsync();

            var owner = Mapper.Map<UserInfo>(await UserService.GetUserById(timelineEntity.OwnerId));

            var members = new List<UserInfo>();
            foreach (var memberEntity in timelineMemberEntities)
            {
                members.Add(Mapper.Map<UserInfo>(await UserService.GetUserById(memberEntity.UserId)));
            }

            return new BaseTimelineInfo
            {
                Description = timelineEntity.Description ?? "",
                Owner = owner,
                Visibility = timelineEntity.Visibility,
                Members = members
            };
        }

    }
}
