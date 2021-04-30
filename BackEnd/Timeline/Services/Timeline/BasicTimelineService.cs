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
    public class BasicTimelineService : IBasicTimelineService
    {
        private readonly ILogger<BasicTimelineService> _logger;

        private readonly DatabaseContext _database;

        private readonly IBasicUserService _basicUserService;
        private readonly IClock _clock;

        private readonly GeneralTimelineNameValidator _generalTimelineNameValidator = new GeneralTimelineNameValidator();

        public BasicTimelineService(ILoggerFactory loggerFactory, DatabaseContext database, IBasicUserService basicUserService, IClock clock)
        {
            _logger = loggerFactory.CreateLogger<BasicTimelineService>();
            _database = database;
            _basicUserService = basicUserService;
            _clock = clock;
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
                    userId = await _basicUserService.GetUserIdByUsernameAsync(username);
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
                var timelineEntity = await _database.Timelines.Where(t => t.Name == timelineName).Select(t => new { t.Id }).SingleOrDefaultAsync();

                if (timelineEntity == null)
                {
                    throw CreateTimelineNotExistException(timelineName);
                }
                else
                {
                    return timelineEntity.Id;
                }
            }
        }
    }
}
