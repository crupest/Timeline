using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Services.Api
{

    [Serializable]
    public class InvalidHighlightTimelineException : Exception
    {
        public InvalidHighlightTimelineException() { }
        public InvalidHighlightTimelineException(string message) : base(message) { }
        public InvalidHighlightTimelineException(string message, Exception inner) : base(message, inner) { }
        protected InvalidHighlightTimelineException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Service that controls highlight timeline.
    /// </summary>
    public interface IHighlightTimelineService
    {
        /// <summary>
        /// Get all highlight timelines in order.
        /// </summary>
        /// <returns>Id list of all highlight timelines.</returns>
        Task<List<long>> GetHighlightTimelines();

        /// <summary>
        /// Check if a timeline is highlight timeline.
        /// </summary>
        /// <param name="timelineId">Timeline id.</param>
        /// <param name="checkTimelineExistence">If true it will throw if timeline does not exist.</param>
        /// <returns>True if timeline is highlight. Otherwise false.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist and <paramref name="checkTimelineExistence"/> is true.</exception>
        Task<bool> IsHighlightTimeline(long timelineId, bool checkTimelineExistence = true);

        /// <summary>
        /// Add a timeline to highlight list.
        /// </summary>
        /// <param name="timelineId">The timeline id.</param>
        /// <param name="operatorId">The user id of operator.</param>
        /// <returns>True if timeline is actually added to highligh. False if it already is.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given id does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown when user with given operator id does not exist.</exception>
        Task<bool> AddHighlightTimeline(long timelineId, long? operatorId);

        /// <summary>
        /// Remove a timeline from highlight list.
        /// </summary>
        /// <param name="timelineId">The timeline id.</param>
        /// <param name="operatorId">The user id of operator.</param>
        /// <returns>True if deletion is actually performed. Otherwise false (timeline was not in the list).</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given id does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown when user with given operator id does not exist.</exception>
        Task<bool> RemoveHighlightTimeline(long timelineId, long? operatorId);

        /// <summary>
        /// Move a highlight timeline to a new position.
        /// </summary>
        /// <param name="timelineId">The timeline name.</param>
        /// <param name="newPosition">The new position. Starts at 1.</param>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given id does not exist.</exception>
        /// <exception cref="InvalidHighlightTimelineException">Thrown when given timeline is not a highlight timeline.</exception>
        /// <remarks>
        /// If <paramref name="newPosition"/> is smaller than 1. Then move the timeline to head.
        /// If <paramref name="newPosition"/> is bigger than total count. Then move the timeline to tail.
        /// </remarks>
        Task MoveHighlightTimeline(long timelineId, long newPosition);
    }

    public class HighlightTimelineService : IHighlightTimelineService
    {
        private readonly DatabaseContext _database;
        private readonly IBasicUserService _userService;
        private readonly IBasicTimelineService _timelineService;
        private readonly IClock _clock;

        public HighlightTimelineService(DatabaseContext database, IBasicUserService userService, IBasicTimelineService timelineService, IClock clock)
        {
            _database = database;
            _userService = userService;
            _timelineService = timelineService;
            _clock = clock;
        }

        public async Task<bool> AddHighlightTimeline(long timelineId, long? operatorId)
        {
            if (!await _timelineService.CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);

            if (operatorId.HasValue && !await _userService.CheckUserExistence(operatorId.Value))
            {
                throw new UserNotExistException(null, operatorId.Value, "User with given operator id does not exist.", null);
            }

            var alreadyIs = await _database.HighlightTimelines.AnyAsync(t => t.TimelineId == timelineId);

            if (alreadyIs) return false;

            _database.HighlightTimelines.Add(new HighlightTimelineEntity { TimelineId = timelineId, OperatorId = operatorId, AddTime = _clock.GetCurrentTime(), Order = await _database.HighlightTimelines.CountAsync() + 1 });
            await _database.SaveChangesAsync();
            return true;
        }

        public async Task<List<long>> GetHighlightTimelines()
        {
            var entities = await _database.HighlightTimelines.OrderBy(t => t.Order).Select(t => new { t.TimelineId }).ToListAsync();

            return entities.Select(e => e.TimelineId).ToList();
        }

        public async Task<bool> RemoveHighlightTimeline(long timelineId, long? operatorId)
        {
            if (!await _timelineService.CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);

            if (operatorId.HasValue && !await _userService.CheckUserExistence(operatorId.Value))
            {
                throw new UserNotExistException(null, operatorId.Value, "User with given operator id does not exist.", null);
            }

            var entity = await _database.HighlightTimelines.SingleOrDefaultAsync(t => t.TimelineId == timelineId);

            if (entity == null) return false;

            await using var transaction = await _database.Database.BeginTransactionAsync();

            var order = entity.Order;

            _database.HighlightTimelines.Remove(entity);
            await _database.SaveChangesAsync();

            await _database.Database.ExecuteSqlRawAsync("UPDATE highlight_timelines SET `order` = `order` - 1 WHERE `order` > {0}", order);

            await transaction.CommitAsync();

            return true;
        }

        public async Task MoveHighlightTimeline(long timelineId, long newPosition)
        {
            if (!await _timelineService.CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);

            var entity = await _database.HighlightTimelines.SingleOrDefaultAsync(t => t.TimelineId == timelineId);

            if (entity == null) throw new InvalidHighlightTimelineException("You can't move a non-highlight timeline.");

            var oldPosition = entity.Order;

            if (newPosition < 1)
            {
                newPosition = 1;
            }
            else
            {
                var totalCount = await _database.HighlightTimelines.CountAsync();
                if (newPosition > totalCount) newPosition = totalCount;
            }

            if (oldPosition == newPosition) return;

            await using var transaction = await _database.Database.BeginTransactionAsync();

            if (newPosition > oldPosition)
            {
                await _database.Database.ExecuteSqlRawAsync("UPDATE highlight_timelines SET `order` = `order` - 1 WHERE `order` BETWEEN {0} AND {1}", oldPosition + 1, newPosition);
                await _database.Database.ExecuteSqlRawAsync("UPDATE highlight_timelines SET `order` = {0} WHERE id = {1}", newPosition, entity.Id);
            }
            else
            {
                await _database.Database.ExecuteSqlRawAsync("UPDATE highlight_timelines SET `order` = `order` + 1 WHERE `order` BETWEEN {0} AND {1}", newPosition, oldPosition - 1);
                await _database.Database.ExecuteSqlRawAsync("UPDATE highlight_timelines SET `order` = {0} WHERE id = {1}", newPosition, entity.Id);
            }

            await transaction.CommitAsync();
        }

        public async Task<bool> IsHighlightTimeline(long timelineId, bool checkTimelineExistence = true)
        {
            if (checkTimelineExistence && !await _timelineService.CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);

            return await _database.HighlightTimelines.AnyAsync(t => t.TimelineId == timelineId);
        }
    }
}
