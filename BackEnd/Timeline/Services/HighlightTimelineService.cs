using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services.Exceptions;

namespace Timeline.Services
{
    public interface IHighlightTimelineService
    {
        /// <summary>
        /// Get all highlight timelines.
        /// </summary>
        /// <returns>A list of all highlight timelines.</returns>
        Task<List<Models.Timeline>> GetHighlightTimelines();

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
    }

    public class HighlightTimelineService : IHighlightTimelineService
    {
        private readonly DatabaseContext _database;
        private readonly IUserService _userService;
        private readonly ITimelineService _timelineService;

        public HighlightTimelineService(DatabaseContext database, IUserService userService, ITimelineService timelineService)
        {
            _database = database;
            _userService = userService;
            _timelineService = timelineService;
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

            _database.HighlightTimelines.Add(new HighlightTimelineEntity { TimelineId = timelineId, OperatorId = operatorId });
            await _database.SaveChangesAsync();
        }

        public async Task<List<Models.Timeline>> GetHighlightTimelines()
        {
            var entities = await _database.HighlightTimelines.Select(t => new { t.Id }).ToListAsync();

            var result = new List<Models.Timeline>();

            foreach (var entity in entities)
            {
                result.Add(await _timelineService.GetTimelineById(entity.Id));
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

            _database.HighlightTimelines.Remove(entity);
            await _database.SaveChangesAsync();

            return true;
        }
    }
}
