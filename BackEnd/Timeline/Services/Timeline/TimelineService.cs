using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Models.Validation;
using Timeline.Services.User;

namespace Timeline.Services.Timeline
{
    public static class TimelineHelper
    {
        public static string ExtractTimelineName(string name, out bool isPersonal)
        {
            if (name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
            {
                isPersonal = true;
                return name[1..];
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

    public class TimelineChangePropertyParams
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TimelineVisibility? Visibility { get; set; }
        public string? Color { get; set; }
    }

    /// <summary>
    /// This define the interface of both personal timeline and ordinary timeline.
    /// </summary>
    public interface ITimelineService : IBasicTimelineService
    {
        /// <summary>
        /// Get the timeline info.
        /// </summary>
        /// <param name="id">Id of timeline.</param>
        /// <returns>The timeline info.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        Task<TimelineEntity> GetTimeline(long id);

        /// <summary>
        /// Set the properties of a timeline. 
        /// </summary>
        /// <param name="id">The id of the timeline.</param>
        /// <param name="newProperties">The new properties. Null member means not to change.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newProperties"/> is null.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given id does not exist.</exception>
        /// <exception cref="EntityAlreadyExistException">Thrown when a timeline with new name already exists.</exception>
        Task ChangeProperty(long id, TimelineChangePropertyParams newProperties);

        /// <summary>
        /// Add a member to timeline.
        /// </summary>
        /// <param name="timelineId">Timeline id.</param>
        /// <param name="userId">User id.</param>
        /// <returns>True if the memeber was added. False if it is already a member.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user does not exist.</exception>
        Task<bool> AddMember(long timelineId, long userId);

        /// <summary>
        /// Remove a member from timeline.
        /// </summary>
        /// <param name="timelineId">Timeline id.</param>
        /// <param name="userId">User id.</param>
        /// <returns>True if the memeber was removed. False if it was not a member before.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user does not exist.</exception>
        Task<bool> RemoveMember(long timelineId, long userId);

        /// <summary>
        /// Check whether a user can manage(change timeline info, member, ...) a timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="userId">The id of the user to check on.</param>
        /// <returns>True if the user can manage the timeline, otherwise false.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <remarks>
        /// This method does not check whether visitor is administrator.
        /// Return false if user with user id does not exist.
        /// </remarks>
        Task<bool> HasManagePermission(long timelineId, long userId);

        /// <summary>
        /// Verify whether a visitor has the permission to read a timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="visitorId">The id of the user to check on. Null means visitor without account.</param>
        /// <returns>True if can read, false if can't read.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <remarks>
        /// This method does not check whether visitor is administrator.
        /// Return false if user with visitor id does not exist.
        /// </remarks>
        Task<bool> HasReadPermission(long timelineId, long? visitorId);

        /// <summary>
        /// Verify whether a user is member of a timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="userId">The id of user to check on.</param>
        /// <returns>True if it is a member, false if not.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <remarks>
        /// Timeline owner is also considered as a member.
        /// Return false when user with user id does not exist.
        /// </remarks>
        Task<bool> IsMemberOf(long timelineId, long userId);

        /// <summary>
        /// Get all timelines including personal and ordinary timelines.
        /// </summary>
        /// <param name="relate">Filter timelines related (own or is a member) to specific user.</param>
        /// <param name="visibility">Filter timelines with given visibility. If null or empty, all visibilities are returned. Duplicate value are ignored.</param>
        /// <returns>The list of timelines.</returns>
        /// <remarks>
        /// If user with related user id does not exist, empty list will be returned.
        /// </remarks>
        Task<List<TimelineEntity>> GetTimelines(TimelineUserRelationship? relate = null, List<TimelineVisibility>? visibility = null);

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
        Task<TimelineEntity> CreateTimeline(string timelineName, long ownerId);

        /// <summary>
        /// Delete a timeline.
        /// </summary>
        /// <param name="id">The id of the timeline to delete.</param>
        /// <exception cref="TimelineNotExistException">Thrown when the timeline does not exist.</exception>
        Task DeleteTimeline(long id);
    }

    public class TimelineService : BasicTimelineService, ITimelineService
    {
        public TimelineService(DatabaseContext database, IBasicUserService userService, IClock clock)
            : base(database, userService, clock)
        {
            _database = database;
            _userService = userService;
            _clock = clock;
        }

        private readonly DatabaseContext _database;

        private readonly IBasicUserService _userService;

        private readonly IClock _clock;

        private readonly TimelineNameValidator _timelineNameValidator = new TimelineNameValidator();

        private readonly ColorValidator _colorValidator = new ColorValidator();

        private void ValidateTimelineName(string name, string paramName)
        {
            if (!_timelineNameValidator.Validate(name, out var message))
            {
                throw new ArgumentException(string.Format(Resource.ExceptionTimelineNameBadFormat, message), paramName);
            }
        }

        public async Task<TimelineEntity> GetTimeline(long id)
        {
            var entity = await _database.Timelines.Where(t => t.Id == id).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelineNotExistException(id);

            return entity;
        }

        public async Task ChangeProperty(long id, TimelineChangePropertyParams newProperties)
        {
            if (newProperties is null)
                throw new ArgumentNullException(nameof(newProperties));

            if (newProperties.Name is not null)
                ValidateTimelineName(newProperties.Name, nameof(newProperties));

            if (newProperties.Color is not null)
            {
                var (result, message) = _colorValidator.Validate(newProperties.Color);
                if (!result)
                {
                    throw new ArgumentException(message, nameof(newProperties));
                }
            }

            var entity = await _database.Timelines.Where(t => t.Id == id).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelineNotExistException(id);

            var changed = false;
            var nameChanged = false;

            if (newProperties.Name is not null)
            {
                var conflict = await _database.Timelines.AnyAsync(t => t.Name == newProperties.Name);

                if (conflict)
                    throw new TimelineAlreadyExistException();

                entity.Name = newProperties.Name;

                changed = true;
                nameChanged = true;
            }

            if (newProperties.Title != null)
            {
                changed = true;
                entity.Title = newProperties.Title;
            }

            if (newProperties.Description != null)
            {
                changed = true;
                entity.Description = newProperties.Description;
            }

            if (newProperties.Visibility.HasValue)
            {
                changed = true;
                entity.Visibility = newProperties.Visibility.Value;
            }

            if (newProperties.Color is not null)
            {
                changed = true;
                entity.Color = newProperties.Color;
            }

            if (changed)
            {
                var currentTime = _clock.GetCurrentTime();
                entity.LastModified = currentTime;
                if (nameChanged)
                    entity.NameLastModified = currentTime;
            }

            await _database.SaveChangesAsync();
        }

        public async Task<bool> AddMember(long timelineId, long userId)
        {
            if (!await CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);

            if (!await _userService.CheckUserExistence(userId))
                throw new UserNotExistException(userId);

            if (await _database.TimelineMembers.AnyAsync(m => m.TimelineId == timelineId && m.UserId == userId))
                return false;


            var entity = new TimelineMemberEntity { UserId = userId, TimelineId = timelineId };
            _database.TimelineMembers.Add(entity);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).SingleAsync();
            timelineEntity.LastModified = _clock.GetCurrentTime();

            await _database.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveMember(long timelineId, long userId)
        {
            if (!await CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);

            if (!await _userService.CheckUserExistence(userId))
                throw new UserNotExistException(userId);

            var entity = await _database.TimelineMembers.SingleOrDefaultAsync(m => m.TimelineId == timelineId && m.UserId == userId);
            if (entity is null) return false;

            _database.TimelineMembers.Remove(entity);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).SingleAsync();
            timelineEntity.LastModified = _clock.GetCurrentTime();

            await _database.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasManagePermission(long timelineId, long userId)
        {
            var entity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelineNotExistException(timelineId);

            return entity.OwnerId == userId;
        }

        public async Task<bool> HasReadPermission(long timelineId, long? visitorId)
        {
            var entity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.Visibility }).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelineNotExistException(timelineId);

            if (entity.Visibility == TimelineVisibility.Public)
                return true;

            if (entity.Visibility == TimelineVisibility.Register && visitorId != null)
                return true;

            if (visitorId == null)
            {
                return false;
            }
            else
            {
                var memberEntity = await _database.TimelineMembers.Where(m => m.UserId == visitorId && m.TimelineId == timelineId).SingleOrDefaultAsync();
                return memberEntity is not null;
            }
        }

        public async Task<bool> IsMemberOf(long timelineId, long userId)
        {
            var entity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelineNotExistException(timelineId);

            if (userId == entity.OwnerId)
                return true;

            return await _database.TimelineMembers.AnyAsync(m => m.TimelineId == timelineId && m.UserId == userId);
        }

        public async Task<List<TimelineEntity>> GetTimelines(TimelineUserRelationship? relate = null, List<TimelineVisibility>? visibility = null)
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
                entities = await ApplyTimelineVisibilityFilter(_database.Timelines).ToListAsync();
            }
            else
            {
                entities = new List<TimelineEntity>();

                if ((relate.Type & TimelineUserRelationshipType.Own) != 0)
                {
                    entities.AddRange(await ApplyTimelineVisibilityFilter(_database.Timelines.Where(t => t.OwnerId == relate.UserId)).ToListAsync());
                }

                if ((relate.Type & TimelineUserRelationshipType.Join) != 0)
                {
                    entities.AddRange(await ApplyTimelineVisibilityFilter(_database.TimelineMembers.Where(m => m.UserId == relate.UserId).Include(m => m.Timeline).Select(m => m.Timeline)).ToListAsync());
                }
            }

            return entities;
        }

        public async Task<TimelineEntity> CreateTimeline(string name, long owner)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            ValidateTimelineName(name, nameof(name));

            var conflict = await _database.Timelines.AnyAsync(t => t.Name == name);

            if (conflict)
                throw new TimelineAlreadyExistException();

            var entity = CreateNewTimelineEntity(name, owner);

            _database.Timelines.Add(entity);
            await _database.SaveChangesAsync();

            return entity;
        }

        public async Task DeleteTimeline(long id)
        {
            var entity = await _database.Timelines.Where(t => t.Id == id).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelineNotExistException(id);

            _database.Timelines.Remove(entity);
            await _database.SaveChangesAsync();
        }
    }

    public static class TimelineServiceExtensions
    {
        public static async Task<List<TimelineEntity>> GetTimelineList(this ITimelineService service, IEnumerable<long> ids)
        {
            var timelines = new List<TimelineEntity>();
            foreach (var id in ids)
            {
                timelines.Add(await service.GetTimeline(id));
            }
            return timelines;
        }
    }
}
