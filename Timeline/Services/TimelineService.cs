using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Models.Validation;

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
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
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
        /// <param name="author">The author's username.</param>
        /// <param name="content">The content.</param>
        /// <param name="time">The time of the post. If null, then use current time.</param>
        /// <returns>The info of the created post.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="author"/> or <paramref name="content"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="UsernameBadFormatException">Thrown if <paramref name="author"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown if <paramref name="author"/> does not exist.</exception>
        Task<TimelinePostCreateResponse> CreatePost(string name, string author, string content, DateTime? time);

        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="id">The id of the post to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="username"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
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
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        Task ChangeProperty(string name, TimelinePropertyChangeRequest newProperties);

        /// <summary>
        /// Remove members to a timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="add">A list of usernames of members to add. May be null.</param>
        /// <param name="remove">A list of usernames of members to remove. May be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="TimelineMemberOperationUserException">
        /// Thrown when an exception occurs on the user list.
        /// The inner exception is <see cref="UsernameBadFormatException"/>
        /// when one of the username is invalid.
        /// The inner exception is <see cref="UserNotExistException"/>
        /// when one of the user to change does not exist.
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
        /// <param name="username">The user to check on. Null means visitor without account.</param>
        /// <returns>True if can read, false if can't read.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="UsernameBadFormatException">
        /// Thrown when <paramref name="username"/> is of bad format.
        /// </exception>
        /// <exception cref="UserNotExistException">
        /// Thrown when <paramref name="username"/> does not exist.
        /// </exception>
        Task<bool> HasReadPermission(string name, string? username);

        /// <summary>
        /// Verify whether a user has the permission to modify a post.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="username">The user to check on.</param>
        /// <returns>True if can modify, false if can't modify.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="username"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="TimelinePostNotExistException">
        /// Thrown when the post with given id does not exist or is deleted already.
        /// </exception>
        /// <exception cref="UsernameBadFormatException">
        /// Thrown when <paramref name="username"/> is of bad format.
        /// </exception>
        /// <exception cref="UserNotExistException">
        /// Thrown when <paramref name="username"/> does not exist.
        /// </exception>
        /// <remarks>
        /// This method does not check whether the user is administrator.
        /// It only checks whether he is the author of the post or the owner of the timeline.
        /// </remarks>
        Task<bool> HasPostModifyPermission(string name, long id, string username);

        /// <summary>
        /// Verify whether a user is member of a timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="username">The user to check on.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="username"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="UsernameBadFormatException">
        /// Thrown when <paramref name="username"/> is not a valid username.
        /// </exception>
        /// <exception cref="UserNotExistException">
        /// Thrown when user <paramref name="username"/> does not exist.</exception>
        /// <returns>True if it is a member, false if not.</returns>
        Task<bool> IsMemberOf(string name, string username);
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
        /// <exception cref="TimelineNameBadFormatException">
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
        /// <param name="owner">The owner of the timeline.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="owner"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is invalid. Currently it means it is an empty string.
        /// </exception>
        /// <exception cref="TimelineAlreadyExistException">
        /// Thrown when the timeline already exists.
        /// </exception>
        /// <exception cref="UsernameBadFormatException">
        /// Thrown when the username of the owner is not valid.
        /// </exception>
        /// <exception cref="UserNotExistException">
        /// Thrown when the owner user does not exist.</exception>
        Task CreateTimeline(string name, string owner);
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
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when <paramref name="username"/> is of bad format. Inner exception MUST be <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when the user does not exist. Inner exception MUST be <see cref="UserNotExistException"/>.
        /// </exception>
        Task<BaseTimelineInfo> GetTimeline(string username);
    }

    public abstract class BaseTimelineService : IBaseTimelineService
    {
        protected BaseTimelineService(DatabaseContext database, IClock clock)
        {
            Clock = clock;
            Database = database;
        }

        protected IClock Clock { get; }

        protected UsernameValidator UsernameValidator { get; } = new UsernameValidator();

        protected DatabaseContext Database { get; }

        /// <summary>
        /// Find the timeline id by the name.
        /// For details, see remarks.
        /// </summary>
        /// <param name="name">The username or the timeline name. See remarks.</param>
        /// <returns>The id of the timeline entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
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
            var postEntities = await Database.TimelinePosts.Where(p => p.TimelineId == timelineId).ToListAsync();
            var posts = new List<TimelinePostInfo>(await Task.WhenAll(postEntities.Select(async p => new TimelinePostInfo
            {
                Id = p.Id,
                Content = p.Content,
                Author = (await Database.Users.Where(u => u.Id == p.AuthorId).Select(u => new { u.Name }).SingleAsync()).Name,
                Time = p.Time
            })));
            return posts;
        }

        public async Task<TimelinePostCreateResponse> CreatePost(string name, string author, string content, DateTime? time)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (author == null)
                throw new ArgumentNullException(nameof(author));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            {
                var (result, message) = UsernameValidator.Validate(author);
                if (!result)
                {
                    throw new UsernameBadFormatException(author, message);
                }
            }

            var timelineId = await FindTimelineId(name);

            var authorEntity = Database.Users.Where(u => u.Name == author).Select(u => new { u.Id }).SingleOrDefault();
            if (authorEntity == null)
            {
                throw new UserNotExistException(author);
            }
            var authorId = authorEntity.Id;

            var currentTime = Clock.GetCurrentTime();

            var postEntity = new TimelinePostEntity
            {
                Content = content,
                AuthorId = authorId,
                TimelineId = timelineId,
                Time = time ?? currentTime,
                LastUpdated = currentTime
            };

            Database.TimelinePosts.Add(postEntity);
            await Database.SaveChangesAsync();

            return new TimelinePostCreateResponse
            {
                Id = postEntity.Id,
                Time = postEntity.Time
            };
        }

        public async Task DeletePost(string name, long id)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var timelineId = FindTimelineId(name);

            var post = await Database.TimelinePosts.Where(p => p.Id == id).SingleOrDefaultAsync();

            if (post == null)
                throw new TimelinePostNotExistException(id);

            Database.TimelinePosts.Remove(post);
            await Database.SaveChangesAsync();
        }

        public async Task ChangeProperty(string name, TimelinePropertyChangeRequest newProperties)
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

            // remove duplication and check the format of each username.
            // Return a username->index map.
            Dictionary<string, int>? RemoveDuplicateAndCheckFormat(IList<string>? list, TimelineMemberOperationUserException.MemberOperation operation)
            {
                if (list != null)
                {
                    Dictionary<string, int> result = new Dictionary<string, int>();
                    var count = 0;
                    for (var index = 0; index < count; index++)
                    {
                        var username = list[index];
                        if (result.ContainsKey(username))
                        {
                            continue;
                        }
                        var (validationResult, message) = UsernameValidator.Validate(username);
                        if (!validationResult)
                            throw new TimelineMemberOperationUserException(
                                index, operation, username,
                                new UsernameBadFormatException(username, message));
                        result.Add(username, index);
                    }
                    return result;
                }
                else
                {
                    return null;
                }
            }
            var simplifiedAdd = RemoveDuplicateAndCheckFormat(add, TimelineMemberOperationUserException.MemberOperation.Add);
            var simplifiedRemove = RemoveDuplicateAndCheckFormat(remove, TimelineMemberOperationUserException.MemberOperation.Remove);

            // remove those both in add and remove
            if (simplifiedAdd != null && simplifiedRemove != null)
            {
                var usersToClean = simplifiedRemove.Keys.Where(u => simplifiedAdd.ContainsKey(u));
                foreach (var u in usersToClean)
                {
                    simplifiedAdd.Remove(u);
                    simplifiedRemove.Remove(u);
                }
            }

            var timelineId = await FindTimelineId(name);

            async Task<List<long>?> CheckExistenceAndGetId(Dictionary<string, int>? map, TimelineMemberOperationUserException.MemberOperation operation)
            {
                if (map == null)
                    return null;

                List<long> result = new List<long>();
                foreach (var (username, index) in map)
                {
                    var user = await Database.Users.Where(u => u.Name == username).Select(u => new { u.Id }).SingleOrDefaultAsync();
                    if (user == null)
                    {
                        throw new TimelineMemberOperationUserException(index, operation, username,
                            new UserNotExistException(username));
                    }
                    result.Add(user.Id);
                }
                return result;
            }
            var userIdsAdd = await CheckExistenceAndGetId(simplifiedAdd, TimelineMemberOperationUserException.MemberOperation.Add);
            var userIdsRemove = await CheckExistenceAndGetId(simplifiedRemove, TimelineMemberOperationUserException.MemberOperation.Remove);

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

        public async Task<bool> HasReadPermission(string name, string? username)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            long? userId = null;
            if (username != null)
            {
                var (result, message) = UsernameValidator.Validate(username);
                if (!result)
                {
                    throw new UsernameBadFormatException(username);
                }

                var user = await Database.Users.Where(u => u.Name == username).Select(u => new { u.Id }).SingleOrDefaultAsync();

                if (user == null)
                {
                    throw new UserNotExistException(username);
                }

                userId = user.Id;
            }

            var timelineId = await FindTimelineId(name);

            var timelineEntity = await Database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.Visibility }).SingleAsync();

            if (timelineEntity.Visibility == TimelineVisibility.Public)
                return true;

            if (timelineEntity.Visibility == TimelineVisibility.Register && username != null)
                return true;

            if (userId == null)
            {
                return false;
            }
            else
            {
                var memberEntity = await Database.TimelineMembers.Where(m => m.UserId == userId && m.TimelineId == timelineId).SingleOrDefaultAsync();
                return memberEntity != null;
            }
        }

        public async Task<bool> HasPostModifyPermission(string name, long id, string username)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (username == null)
                throw new ArgumentNullException(nameof(username));

            {
                var (result, message) = UsernameValidator.Validate(username);
                if (!result)
                {
                    throw new UsernameBadFormatException(username);
                }
            }

            var user = await Database.Users.Where(u => u.Name == username).Select(u => new { u.Id }).SingleOrDefaultAsync();

            if (user == null)
            {
                throw new UserNotExistException(username);
            }

            var userId = user.Id;

            var timelineId = await FindTimelineId(name);

            var timelineEntity = await Database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleAsync();

            var postEntitu = await Database.Timelines. // TODO!

            if (timelineEntity.OwnerId == userId)
            {
                return true;
            }
        }

    }
}
