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

    public class TimelineService : ITimelineService
    {
        private readonly ILogger<TimelineService> _logger;

        private readonly DatabaseContext _database;

        private readonly IUserService _userService;

        private readonly IClock _clock;

        private readonly GeneralTimelineNameValidator _generalTimelineNameValidator = new GeneralTimelineNameValidator();
        private readonly TimelineNameValidator _timelineNameValidator = new TimelineNameValidator();
        private readonly ColorValidator _colorValidator = new ColorValidator() { PermitDefault = true, PermitEmpty = true };

        public TimelineService(ILogger<TimelineService> logger, DatabaseContext database, IUserService userService, IClock clock)
        {
            _logger = logger;
            _database = database;
            _userService = userService;
            _clock = clock;
        }

        private static EntityAlreadyExistException CreateTimelineConflictException(long ownerId, string timelineName)
        {
            return new EntityAlreadyExistException(EntityTypes.Timeline, new Dictionary<string, object>
            {
                [nameof(ownerId)] = ownerId,
                [nameof(timelineName)] = timelineName
            });
        }

        private void CheckTimelineName(string name, string paramName)
        {
            if (!_timelineNameValidator.Validate(name, out var message))
            {
                throw new ArgumentException(string.Format(Resource.ExceptionTimelineNameBadFormat, message), paramName);
            }
        }

        protected TimelineEntity CreateNewTimelineEntity(string? name, long ownerId)
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

        protected static EntityNotExistException CreateTimelineNotExistException(string name, Exception? inner = null)
        {
            return new EntityNotExistException(EntityTypes.Timeline, new Dictionary<string, object>
            {
                ["name"] = name
            }, null, inner);
        }

        protected static EntityNotExistException CreateTimelineNotExistException(long id)
        {
            return new EntityNotExistException(EntityTypes.Timeline, new Dictionary<string, object>
            {
                ["id"] = id
            });
        }

        protected static EntityNotExistException CreateTimelineNotExistException(long ownerId, string timelineName)
        {
            return new EntityNotExistException(EntityTypes.Timeline, new Dictionary<string, object>
            {
                [nameof(ownerId)] = ownerId,
                [nameof(timelineName)] = timelineName
            });
        }

        protected void CheckGeneralTimelineName(string timelineName, string? paramName)
        {
            if (!_generalTimelineNameValidator.Validate(timelineName, out var message))
                throw new ArgumentException(string.Format(Resource.ExceptionGeneralTimelineNameBadFormat, message), paramName);
        }

        public async Task<bool> CheckTimelineExistenceAsync(long id)
        {
            return await _database.Timelines.AnyAsync(t => t.Id == id);
        }

        public async Task<long> GetTimelineIdByNameAsync(string timelineName)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            CheckGeneralTimelineName(timelineName, nameof(timelineName));

            var name = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);

            if (isPersonal)
            {
                var username = name;
                long userId;
                try
                {
                    userId = await _userService.GetUserIdByUsernameAsync(username);
                }
                catch (EntityNotExistException e)
                {
                    throw CreateTimelineNotExistException(timelineName, e);
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

                    _logger.LogInformation(Resource.LogPersonalTimelineAutoCreate, username);

                    return newTimelineEntity.Id;
                }
            }
            else
            {
                var timelineEntities = await _database.Timelines.Where(t => t.Name == timelineName).Select(t => new { t.Id }).ToListAsync();

                if (timelineEntities.Count == 0)
                {
                    throw CreateTimelineNotExistException(timelineName);
                }
                else if (timelineEntities.Count == 1)
                {
                    return timelineEntities[0].Id;
                }
                else
                {
                    throw new MultipleTimelineException(String.Format("Multiple timelines have name '{}'.", timelineName));
                }
            }
        }


        public async Task<TimelineEntity> GetTimelineAsync(long id)
        {
            var entity = await _database.Timelines.Where(t => t.Id == id).SingleOrDefaultAsync();

            if (entity is null)
                throw CreateTimelineNotExistException(id);

            return entity;
        }

        public async Task ChangePropertyAsync(long id, TimelineChangePropertyParams newProperties)
        {
            if (newProperties is null)
                throw new ArgumentNullException(nameof(newProperties));

            if (newProperties.Name is not null)
                CheckTimelineName(newProperties.Name, nameof(newProperties));

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
                throw CreateTimelineNotExistException(id);

            var changed = false;
            var nameChanged = false;

            if (newProperties.Name is not null)
            {
                var conflict = await _database.Timelines.AnyAsync(t => t.OwnerId == entity.OwnerId && t.Name == newProperties.Name);

                if (conflict)
                    throw CreateTimelineConflictException(entity.OwnerId, newProperties.Name);

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
                if (newProperties.Color.Length == 0 || newProperties.Color == "default")
                {
                    entity.Color = null;
                }
                else
                {
                    entity.Color = newProperties.Color;
                }
            }

            if (changed)
            {
                var currentTime = _clock.GetCurrentTime();
                entity.LastModified = currentTime;
                if (nameChanged)
                    entity.NameLastModified = currentTime;
            }

            await _database.SaveChangesAsync();
            _logger.LogInformation(Resource.LogTimelineUpdated, id);
        }

        public async Task<bool> AddMemberAsync(long timelineId, long userId)
        {
            if (!await CheckTimelineExistenceAsync(timelineId))
                throw CreateTimelineNotExistException(timelineId);

            await _userService.ThrowIfUserNotExist(userId);

            if (await _database.TimelineMembers.AnyAsync(m => m.TimelineId == timelineId && m.UserId == userId))
                return false;

            var entity = new TimelineMemberEntity { UserId = userId, TimelineId = timelineId };
            _database.TimelineMembers.Add(entity);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).SingleAsync();
            timelineEntity.LastModified = _clock.GetCurrentTime();

            await _database.SaveChangesAsync();
            _logger.LogInformation(Resource.LogTimelineAddMember, userId, timelineId);

            return true;
        }

        public async Task<bool> RemoveMemberAsync(long timelineId, long userId)
        {
            if (!await CheckTimelineExistenceAsync(timelineId))
                throw CreateTimelineNotExistException(timelineId);

            await _userService.ThrowIfUserNotExist(userId);

            var entity = await _database.TimelineMembers.SingleOrDefaultAsync(m => m.TimelineId == timelineId && m.UserId == userId);
            if (entity is null) return false;

            _database.TimelineMembers.Remove(entity);

            var timelineEntity = await _database.Timelines.Where(t => t.Id == timelineId).SingleAsync();
            timelineEntity.LastModified = _clock.GetCurrentTime();

            await _database.SaveChangesAsync();
            _logger.LogInformation(Resource.LogTimelineRemoveMember, userId, timelineId);

            return true;
        }

        public async Task<bool> HasManagePermissionAsync(long timelineId, long userId)
        {
            var entity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId }).SingleOrDefaultAsync();

            if (entity is null)
                throw CreateTimelineNotExistException(timelineId);

            return entity.OwnerId == userId;
        }

        public async Task<bool> HasReadPermissionAsync(long timelineId, long? visitorId)
        {
            var entity = await _database.Timelines.Where(t => t.Id == timelineId).Select(t => new { t.OwnerId, t.Visibility }).SingleOrDefaultAsync();

            if (entity is null)
                throw CreateTimelineNotExistException(timelineId);

            if (entity.Visibility == TimelineVisibility.Public)
                return true;

            if (entity.Visibility == TimelineVisibility.Register && visitorId != null)
                return true;

            if (visitorId == null)
            {
                return false;
            }
            else if (visitorId == entity.OwnerId)
            {
                return true;
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
                throw CreateTimelineNotExistException(timelineId);

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

        public async Task<TimelineEntity> CreateTimelineAsync(long ownerId, string timelineName)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            CheckTimelineName(timelineName, nameof(timelineName));
            if (timelineName == "self")
            {
                throw new ArgumentException("Timeline name can't be 'self'.");
            }

            var conflict = await _database.Timelines.AnyAsync(t => t.OwnerId == ownerId && t.Name == timelineName);

            if (conflict)
                throw CreateTimelineConflictException(ownerId, timelineName);

            var entity = CreateNewTimelineEntity(timelineName, ownerId);

            _database.Timelines.Add(entity);
            await _database.SaveChangesAsync();
            _logger.LogInformation(Resource.LogTimelineCreate, timelineName, entity.Id);

            return entity;
        }

        public async Task DeleteTimelineAsync(long id)
        {
            var entity = await _database.Timelines.Where(t => t.Id == id).SingleOrDefaultAsync();

            if (entity is null)
                throw CreateTimelineNotExistException(id);

            _database.Timelines.Remove(entity);
            await _database.SaveChangesAsync();
            _logger.LogWarning(Resource.LogTimelineDelete, id);
        }

        public async Task<long> GetTimelineIdAsync(long ownerId, string timelineName)
        {
            if (timelineName is null)
                throw new ArgumentNullException(nameof(timelineName));
            CheckTimelineName(timelineName, nameof(timelineName));

            string? tn = timelineName == "self" ? null : timelineName;

            var entity = await _database.Timelines.Where(t => t.OwnerId == ownerId && t.Name == tn).SingleOrDefaultAsync();
            if (entity is null)
                throw CreateTimelineNotExistException(ownerId, timelineName);

            return entity.Id;
        }

        public async Task<long> GetTimelineIdAsync(string ownerUsername, string timelineName)
        {
            var ownerId = await _userService.GetUserIdByUsernameAsync(ownerUsername);
            return await GetTimelineIdAsync(ownerId, timelineName);
        }
    }
}
