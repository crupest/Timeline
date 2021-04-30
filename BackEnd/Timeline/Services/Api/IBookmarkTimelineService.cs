using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Services.Api
{
    /// <summary>
    /// Service interface that manages timeline bookmarks.
    /// </summary>
    public interface IBookmarkTimelineService
    {
        /// <summary>
        /// Get bookmarks of a user.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <returns>Id of Bookmark timelines in order.</returns>
        /// <exception cref="EntityNotExistException">Thrown when user does not exist.</exception>
        Task<List<long>> GetBookmarksAsync(long userId);

        /// <summary>
        /// Check if a timeline is a bookmark.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="timelineId">Timeline id.</param>
        /// <param name="checkUserExistence">If true it will throw when user does not exist.</param>
        /// <param name="checkTimelineExistence">If true it will throw when timeline does not exist.</param>
        /// <returns>True if timeline is a bookmark. Otherwise false.</returns>
        /// <exception cref="EntityNotExistException">Throw if user does not exist and <paramref name="checkUserExistence"/> is true.</exception>
        /// <exception cref="EntityNotExistException">Thrown if timeline does not exist and <paramref name="checkTimelineExistence"/> is true.</exception>
        Task<bool> IsBookmarkAsync(long userId, long timelineId, bool checkUserExistence = true, bool checkTimelineExistence = true);

        /// <summary>
        /// Add a bookmark to tail to a user.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <param name="timelineId">Timeline id.</param>
        /// <returns>True if timeline is added to bookmark. False if it already is.</returns>
        /// <exception cref="EntityNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        Task<bool> AddBookmarkAsync(long userId, long timelineId);

        /// <summary>
        /// Remove a bookmark from a user.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <param name="timelineId">Timeline id.</param>
        /// <returns>True if deletion is performed. False if bookmark does not exist.</returns>
        /// <exception cref="EntityNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        Task<bool> RemoveBookmarkAsync(long userId, long timelineId);

        /// <summary>
        /// Move bookmark to a new position.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <param name="timelineId">Timeline name.</param>
        /// <param name="newPosition">New position. Starts at 1.</param>
        /// <exception cref="EntityNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="InvalidBookmarkException">Thrown when the timeline is not a bookmark.</exception>
        Task MoveBookmarkAsync(long userId, long timelineId, long newPosition);
    }
}
