using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Services.Exceptions;

namespace Timeline.Services
{

    [Serializable]
    public class InvalidBookmarkException : Exception
    {
        public InvalidBookmarkException() { }
        public InvalidBookmarkException(string message) : base(message) { }
        public InvalidBookmarkException(string message, Exception inner) : base(message, inner) { }
        protected InvalidBookmarkException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Service interface that manages timeline bookmarks.
    /// </summary>
    public interface IBookmarkTimelineService
    {
        /// <summary>
        /// Get bookmarks of a user.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <returns>Bookmarks in order.</returns>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        Task<List<TimelineInfo>> GetBookmarks(long userId);

        /// <summary>
        /// Add a bookmark to tail to a user.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <param name="timelineName">Timeline name.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timelineName"/> is not a valid name.</exception>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        Task AddBookmark(long userId, string timelineName);

        /// <summary>
        /// Remove a bookmark from a user.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <param name="timelineName">Timeline name.</param>
        /// <returns>True if deletion is performed. False if bookmark does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timelineName"/> is not a valid name.</exception>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        Task<bool> RemoveBookmark(long userId, string timelineName);

        /// <summary>
        /// Move bookmark to a new position.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <param name="timelineName">Timeline name.</param>
        /// <param name="position">New position. Starts at 1.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timelineName"/> is not a valid name.</exception>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="InvalidBookmarkException">Thrown when the timeline is not a bookmark.</exception>
        Task MoveBookmark(long userId, string timelineName, long position);
    }

    public class BookmarkTimelineService
    {
    }
}
