using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Services.Api
{
    /// <summary>
    /// Service that controls highlight timeline.
    /// </summary>
    public interface IHighlightTimelineService
    {
        /// <summary>
        /// Get all highlight timelines in order.
        /// </summary>
        /// <returns>Id list of all highlight timelines.</returns>
        Task<List<long>> GetHighlightTimelinesAsync();

        /// <summary>
        /// Check if a timeline is highlight timeline.
        /// </summary>
        /// <param name="timelineId">Timeline id.</param>
        /// <param name="checkTimelineExistence">If true it will throw if timeline does not exist.</param>
        /// <returns>True if timeline is highlight. Otherwise false.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist and <paramref name="checkTimelineExistence"/> is true.</exception>
        Task<bool> IsHighlightTimelineAsync(long timelineId, bool checkTimelineExistence = true);

        /// <summary>
        /// Add a timeline to highlight list.
        /// </summary>
        /// <param name="timelineId">The timeline id.</param>
        /// <param name="operatorId">The user id of operator.</param>
        /// <returns>True if timeline is actually added to highligh. False if it already is.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given id does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown when user with given operator id does not exist.</exception>
        Task<bool> AddHighlightTimelineAsync(long timelineId, long? operatorId);

        /// <summary>
        /// Remove a timeline from highlight list.
        /// </summary>
        /// <param name="timelineId">The timeline id.</param>
        /// <param name="operatorId">The user id of operator.</param>
        /// <returns>True if deletion is actually performed. Otherwise false (timeline was not in the list).</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given id does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown when user with given operator id does not exist.</exception>
        Task<bool> RemoveHighlightTimelineAsync(long timelineId, long? operatorId);

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
        Task MoveHighlightTimelineAsync(long timelineId, long newPosition);
    }
}
