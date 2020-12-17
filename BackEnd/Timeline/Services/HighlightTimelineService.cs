using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Services.Exceptions;

namespace Timeline.Services
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
        /// <returns>A list of all highlight timelines.</returns>
        Task<List<TimelineInfo>> GetHighlightTimelines();

        /// <summary>
        /// Add a timeline to highlight list.
        /// </summary>
        /// <param name="timelineName">The timeline name.</param>
        /// <param name="operatorId">The user id of operator.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timelineName"/> is not a valid timeline name.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given name does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown when user with given operator id does not exist.</exception>
        Task AddHighlightTimeline(string timelineName, long? operatorId);

        /// <summary>
        /// Remove a timeline from highlight list.
        /// </summary>
        /// <param name="timelineName">The timeline name.</param>
        /// <param name="operatorId">The user id of operator.</param>
        /// <returns>True if deletion is actually performed. Otherwise false (timeline was not in the list).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timelineName"/> is not a valid timeline name.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given name does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown when user with given operator id does not exist.</exception>
        Task<bool> RemoveHighlightTimeline(string timelineName, long? operatorId);

        /// <summary>
        /// Move a highlight timeline to a new position.
        /// </summary>
        /// <param name="timelineName">The timeline name.</param>
        /// <param name="newPosition">The new position. Starts at 1.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timelineName"/> is not a valid timeline name.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given name does not exist.</exception>
        /// <exception cref="InvalidHighlightTimelineException">Thrown when given timeline is not a highlight timeline.</exception>
        /// <remarks>
        /// If <paramref name="newPosition"/> is smaller than 1. Then move the timeline to head.
        /// If <paramref name="newPosition"/> is bigger than total count. Then move the timeline to tail.
        /// </remarks>
        Task MoveHighlightTimeline(string timelineName, long newPosition);
    }

    public class HighlightTimelineService : IHighlightTimelineService
    {
        private readonly DatabaseContext _database;
        private readonly IBasicUserService _userService;
        private readonly ITimelineService _timelineService;
        private readonly IClock _clock;

        public HighlightTimelineService(DatabaseContext database, IBasicUserService userService, ITimelineService timelineService, IClock clock)
        {
            _database = database;
            _userService = userService;
            _timelineService = timelineService;
            _clock = clock;
        }

        public async Task AddHighlightTimeline(string timelineName, long? operatorId)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await _timelineService.GetTimelineIdByName(timelineName);

            if (operatorId.HasValue && !await _userService.CheckUserExistence(operatorId.Value))
            {
                throw new UserNotExistException(null, operatorId.Value, "User with given operator id does not exist.", null);
            }

            var alreadyIs = await _database.HighlightTimelines.AnyAsync(t => t.TimelineId == timelineId);

            if (alreadyIs) return;

            _database.HighlightTimelines.Add(new HighlightTimelineEntity { TimelineId = timelineId, OperatorId = operatorId, AddTime = _clock.GetCurrentTime(), Order = await _database.HighlightTimelines.CountAsync() + 1 });
            await _database.SaveChangesAsync();
        }

        public async Task<List<TimelineInfo>> GetHighlightTimelines()
        {
            var entities = await _database.HighlightTimelines.OrderBy(t => t.Order).Select(t => new { t.TimelineId }).ToListAsync();

            var result = new List<TimelineInfo>();

            foreach (var entity in entities)
            {
                result.Add(await _timelineService.GetTimelineById(entity.TimelineId));
            }

            return result;
        }

        public async Task<bool> RemoveHighlightTimeline(string timelineName, long? operatorId)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await _timelineService.GetTimelineIdByName(timelineName);

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

        public async Task MoveHighlightTimeline(string timelineName, long newPosition)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await _timelineService.GetTimelineIdByName(timelineName);

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
    }
}
