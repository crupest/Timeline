using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
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
        /// <returns>Id of Bookmark timelines in order.</returns>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        Task<List<long>> GetBookmarks(long userId);

        /// <summary>
        /// Check if a timeline is a bookmark.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="timelineId">Timeline id.</param>
        /// <param name="checkUserExistence">If true it will throw when user does not exist.</param>
        /// <param name="checkTimelineExistence">If true it will throw when timeline does not exist.</param>
        /// <returns>True if timeline is a bookmark. Otherwise false.</returns>
        /// <exception cref="UserNotExistException">Throw if user does not exist and <paramref name="checkUserExistence"/> is true.</exception>
        /// <exception cref="TimelineNotExistException">Thrown if timeline does not exist and <paramref name="checkTimelineExistence"/> is true.</exception>
        Task<bool> IsBookmark(long userId, long timelineId, bool checkUserExistence = true, bool checkTimelineExistence = true);

        /// <summary>
        /// Add a bookmark to tail to a user.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <param name="timelineId">Timeline id.</param>
        /// <returns>True if timeline is added to bookmark. False if it already is.</returns>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        Task<bool> AddBookmark(long userId, long timelineId);

        /// <summary>
        /// Remove a bookmark from a user.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <param name="timelineId">Timeline id.</param>
        /// <returns>True if deletion is performed. False if bookmark does not exist.</returns>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        Task<bool> RemoveBookmark(long userId, long timelineId);

        /// <summary>
        /// Move bookmark to a new position.
        /// </summary>
        /// <param name="userId">User id of bookmark owner.</param>
        /// <param name="timelineId">Timeline name.</param>
        /// <param name="newPosition">New position. Starts at 1.</param>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="InvalidBookmarkException">Thrown when the timeline is not a bookmark.</exception>
        Task MoveBookmark(long userId, long timelineId, long newPosition);
    }

    public class BookmarkTimelineService : IBookmarkTimelineService
    {
        private readonly DatabaseContext _database;
        private readonly IBasicUserService _userService;
        private readonly IBasicTimelineService _timelineService;

        public BookmarkTimelineService(DatabaseContext database, IBasicUserService userService, IBasicTimelineService timelineService)
        {
            _database = database;
            _userService = userService;
            _timelineService = timelineService;
        }

        public async Task<bool> AddBookmark(long userId, long timelineId)
        {
            if (!await _userService.CheckUserExistence(userId))
                throw new UserNotExistException(userId);

            if (!await _timelineService.CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);

            if (await _database.BookmarkTimelines.AnyAsync(t => t.TimelineId == timelineId && t.UserId == userId))
                return false;

            _database.BookmarkTimelines.Add(new BookmarkTimelineEntity
            {
                TimelineId = timelineId,
                UserId = userId,
                Rank = (await _database.BookmarkTimelines.CountAsync(t => t.UserId == userId)) + 1
            });

            await _database.SaveChangesAsync();
            return true;
        }

        public async Task<List<long>> GetBookmarks(long userId)
        {
            if (!await _userService.CheckUserExistence(userId))
                throw new UserNotExistException(userId);

            var entities = await _database.BookmarkTimelines.Where(t => t.UserId == userId).OrderBy(t => t.Rank).Select(t => new { t.TimelineId }).ToListAsync();

            return entities.Select(e => e.TimelineId).ToList();
        }

        public async Task<bool> IsBookmark(long userId, long timelineId, bool checkUserExistence = true, bool checkTimelineExistence = true)
        {
            if (checkUserExistence && !await _userService.CheckUserExistence(userId))
                throw new UserNotExistException(userId);

            if (checkTimelineExistence && !await _timelineService.CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);

            return await _database.BookmarkTimelines.AnyAsync(b => b.TimelineId == timelineId && b.UserId == userId);
        }

        public async Task MoveBookmark(long userId, long timelineId, long newPosition)
        {
            if (!await _userService.CheckUserExistence(userId))
                throw new UserNotExistException(userId);

            if (!await _timelineService.CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);

            var entity = await _database.BookmarkTimelines.SingleOrDefaultAsync(t => t.TimelineId == timelineId && t.UserId == userId);

            if (entity == null) throw new InvalidBookmarkException("You can't move a non-bookmark timeline.");

            var oldPosition = entity.Rank;

            if (newPosition < 1)
            {
                newPosition = 1;
            }
            else
            {
                var totalCount = await _database.BookmarkTimelines.CountAsync(t => t.UserId == userId);
                if (newPosition > totalCount) newPosition = totalCount;
            }

            if (oldPosition == newPosition) return;

            await using var transaction = await _database.Database.BeginTransactionAsync();

            if (newPosition > oldPosition)
            {
                await _database.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = `rank` - 1 WHERE `rank` BETWEEN {0} AND {1} AND `user` = {2}", oldPosition + 1, newPosition, userId);
                await _database.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = {0} WHERE `id` = {1}", newPosition, entity.Id);
            }
            else
            {
                await _database.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = `rank` + 1 WHERE `rank` BETWEEN {0} AND {1} AND `user` = {2}", newPosition, oldPosition - 1, userId);
                await _database.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = {0} WHERE `id` = {1}", newPosition, entity.Id);
            }

            await transaction.CommitAsync();
        }

        public async Task<bool> RemoveBookmark(long userId, long timelineId)
        {
            if (!await _userService.CheckUserExistence(userId))
                throw new UserNotExistException(userId);

            if (!await _timelineService.CheckExistence(timelineId))
                throw new TimelineNotExistException(timelineId);

            var entity = await _database.BookmarkTimelines.SingleOrDefaultAsync(t => t.UserId == userId && t.TimelineId == timelineId);

            if (entity == null) return false;

            await using var transaction = await _database.Database.BeginTransactionAsync();

            var rank = entity.Rank;

            _database.BookmarkTimelines.Remove(entity);
            await _database.SaveChangesAsync();

            await _database.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = `rank` - 1 WHERE `rank` > {0}", rank);

            await transaction.CommitAsync();

            return true;
        }
    }
}
