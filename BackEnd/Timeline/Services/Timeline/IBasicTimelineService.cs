using System;
using System.Threading.Tasks;
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
        Task<bool> CheckTimelineExistenceAsync(long id);

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
        Task<long> GetTimelineIdByNameAsync(string timelineName);
    }
}
