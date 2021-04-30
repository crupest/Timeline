using System;
using System.Threading.Tasks;

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
        /// <exception cref="EntityNotExistException">
        /// Thrown when timeline with name <paramref name="timelineName"/> does not exist.
        /// </exception>
        Task<long> GetTimelineIdByNameAsync(string timelineName);
    }
}
