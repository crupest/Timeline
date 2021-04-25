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
    /// <summary>
    /// This service provide some basic timeline functions, which should be used internally for other services.
    /// </summary>
    public interface IBasicTimelineService
    {
        /// <summary>
        /// Check whether a timeline with given id exists without getting full info.
        /// </summary>
        /// <param name="id">The timeline id.</param>
        /// <returns>True if exist. Otherwise false.</returns>
        Task<bool> CheckExistence(long id);

        /// <summary>
        /// Get the timeline id by name.
        /// </summary>
        /// <param name="timelineName">Timeline name.</param>
        /// <returns>Id of the timeline.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// If it is a personal timeline, then inner exception is <see cref="UserNotExistException"/>.
        /// </exception>
        /// <remarks>
        /// If name is of personal timeline and the timeline does not exist, it will be created if user exists.
        /// If the user does not exist,  <see cref="TimelineNotExistException"/> will be thrown with <see cref="UserNotExistException"/> as inner exception.
        ///</remarks>
        Task<long> GetTimelineIdByName(string timelineName);
    }


    public class BasicTimelineService : IBasicTimelineService
    {
        private readonly DatabaseContext _database;

        private readonly IBasicUserService _basicUserService;
        private readonly IClock _clock;

        private readonly GeneralTimelineNameValidator _generalTimelineNameValidator = new GeneralTimelineNameValidator();

        public BasicTimelineService(DatabaseContext database, IBasicUserService basicUserService, IClock clock)
        {
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

        public async Task<bool> CheckExistence(long id)
        {
            return await _database.Timelines.AnyAsync(t => t.Id == id);
        }

        public async Task<long> GetTimelineIdByName(string timelineName)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            if (!_generalTimelineNameValidator.Validate(timelineName, out var message))
                throw new ArgumentException(message);

            timelineName = TimelineHelper.ExtractTimelineName(timelineName, out var isPersonal);

            if (isPersonal)
            {
                long userId;
                try
                {
                    userId = await _basicUserService.GetUserIdByUsername(timelineName);
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
    }
}
