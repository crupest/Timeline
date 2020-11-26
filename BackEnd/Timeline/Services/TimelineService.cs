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

    /// <summary>
    /// This define the interface of both personal timeline and ordinary timeline.
    /// </summary>
    public interface ITimelineService : IBasicTimelineService
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
        /// Get timeline by id.
        /// </summary>
        /// <param name="id">Id of timeline.</param>
        /// <returns>The timeline.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given id does not exist.</exception>
        Task<Models.Timeline> GetTimelineById(long id);

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

    public class TimelineService : BasicTimelineService, ITimelineService
    {
        public TimelineService(DatabaseContext database, IUserService userService, IClock clock)
            : base(database, userService, clock)
        {
            _database = database;
            _userService = userService;
            _clock = clock;
        }

        private readonly DatabaseContext _database;

        private readonly IUserService _userService;

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

        public async Task<DateTime> GetTimelineLastModifiedTime(string timelineName)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await GetTimelineIdByName(timelineName);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.LastModified }).SingleAsync();

            return timelineEntity.LastModified;
        }

        public async Task<string> GetTimelineUniqueId(string timelineName)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await GetTimelineIdByName(timelineName);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.UniqueId }).SingleAsync();

            return timelineEntity.UniqueId;
        }

        public async Task<Models.Timeline> GetTimeline(string timelineName)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await GetTimelineIdByName(timelineName);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Include(t => t.Members).SingleAsync();

            return await MapTimelineFromEntity(timelineEntity);
        }

        public async Task<Models.Timeline> GetTimelineById(long id)
        {
            var timelineEntity = await _database.Timelines.Where(t => t.Id == id).Include(t => t.Members).SingleOrDefaultAsync();

            if (timelineEntity is null)
                throw new TimelineNotExistException(id);

            return await MapTimelineFromEntity(timelineEntity);
        }

        public async Task ChangeProperty(string timelineName, TimelineChangePropertyRequest newProperties)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));
            if (newProperties == null)
                throw new ArgumentNullException(nameof(newProperties));

            var timelineId = await GetTimelineIdByName(timelineName);

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

            var timelineId = await GetTimelineIdByName(timelineName);

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

            var timelineId = await GetTimelineIdByName(timelineName);
            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleAsync();

            return userId == timelineEntity.OwnerId;
        }

        public async Task<bool> HasReadPermission(string timelineName, long? visitorId)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await GetTimelineIdByName(timelineName);
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

        public async Task<bool> IsMemberOf(string timelineName, long userId)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await GetTimelineIdByName(timelineName);

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
