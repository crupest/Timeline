using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

    public class TimelineService : BasicTimelineService, ITimelineService
    {
        private readonly ILogger<TimelineService> _logger;

        private readonly DatabaseContext _database;

        private readonly IBasicUserService _userService;

        private readonly IClock _clock;

        private readonly TimelineNameValidator _timelineNameValidator = new TimelineNameValidator();

        private readonly ColorValidator _colorValidator = new ColorValidator();

        public TimelineService(ILoggerFactory loggerFactory, DatabaseContext database, IBasicUserService userService, IClock clock)
            : base(loggerFactory, database, userService, clock)
        {
            _logger = loggerFactory.CreateLogger<TimelineService>();
            _database = database;
            _userService = userService;
            _clock = clock;
        }


        private void ValidateTimelineName(string name, string paramName)
        {
            if (!_timelineNameValidator.Validate(name, out var message))
            {
                throw new ArgumentException(string.Format(Resource.ExceptionTimelineNameBadFormat, message), paramName);
            }
        }

        public async Task<TimelineEntity> GetTimelineAsync(long id)
        {
            var entity = await _database.Timelines.Where(t => t.Id == id).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelineNotExistException(id);

            return entity;
        }

        public async Task ChangePropertyAsync(long id, TimelineChangePropertyParams newProperties)
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

        public async Task<bool> AddMemberAsync(long timelineId, long userId)
        {
            if (!await CheckTimelineExistenceAsync(timelineId))
                throw new TimelineNotExistException(timelineId);

            if (!await _userService.CheckUserExistenceAsync(userId))
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

        public async Task<bool> RemoveMemberAsync(long timelineId, long userId)
        {
            if (!await CheckTimelineExistenceAsync(timelineId))
                throw new TimelineNotExistException(timelineId);

            if (!await _userService.CheckUserExistenceAsync(userId))
                throw new UserNotExistException(userId);

            var entity = await _database.TimelineMembers.SingleOrDefaultAsync(m => m.TimelineId == timelineId && m.UserId == userId);
            if (entity is null) return false;

            _database.TimelineMembers.Remove(entity);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).SingleAsync();
            timelineEntity.LastModified = _clock.GetCurrentTime();

            await _database.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasManagePermissionAsync(long timelineId, long userId)
        {
            var entity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelineNotExistException(timelineId);

            return entity.OwnerId == userId;
        }

        public async Task<bool> HasReadPermissionAsync(long timelineId, long? visitorId)
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

        public async Task<bool> IsMemberOfAsync(long timelineId, long userId)
        {
            var entity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelineNotExistException(timelineId);

            if (userId == entity.OwnerId)
                return true;

            return await _database.TimelineMembers.AnyAsync(m => m.TimelineId == timelineId && m.UserId == userId);
        }

        public async Task<List<TimelineEntity>> GetTimelinesAsync(TimelineUserRelationship? relate = null, List<TimelineVisibility>? visibility = null)
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

        public async Task<TimelineEntity> CreateTimelineAsync(string name, long owner)
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

        public async Task DeleteTimelineAsync(long id)
        {
            var entity = await _database.Timelines.Where(t => t.Id == id).SingleOrDefaultAsync();

            if (entity is null)
                throw new TimelineNotExistException(id);

            _database.Timelines.Remove(entity);
            await _database.SaveChangesAsync();
        }
    }
}
