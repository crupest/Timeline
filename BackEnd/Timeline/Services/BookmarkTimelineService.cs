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
        /// <param name="newPosition">New position. Starts at 1.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timelineName"/> is not a valid name.</exception>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="InvalidBookmarkException">Thrown when the timeline is not a bookmark.</exception>
        Task MoveBookmark(long userId, string timelineName, long newPosition);
    }

    public class BookmarkTimelineService : IBookmarkTimelineService
    {
        private readonly DatabaseContext _database;
        private readonly IBasicUserService _userService;
        private readonly ITimelineService _timelineService;

        public BookmarkTimelineService(DatabaseContext database, IBasicUserService userService, ITimelineService timelineService)
        {
            _database = database;
            _userService = userService;
            _timelineService = timelineService;
        }

        public async Task AddBookmark(long userId, string timelineName)
        {
            if (timelineName is null)
                throw new ArgumentNullException(nameof(timelineName));

            if (!await _userService.CheckUserExistence(userId))
                throw new UserNotExistException(userId);

            var timelineId = await _timelineService.GetTimelineIdByName(timelineName);

            _database.BookmarkTimelines.Add(new BookmarkTimelineEntity
            {
                TimelineId = timelineId,
                UserId = userId,
                Rank = (await _database.BookmarkTimelines.CountAsync(t => t.UserId == userId)) + 1
            });

            await _database.SaveChangesAsync();
        }

        public async Task<List<TimelineInfo>> GetBookmarks(long userId)
        {
            if (!await _userService.CheckUserExistence(userId))
                throw new UserNotExistException(userId);

            var entities = await _database.BookmarkTimelines.Where(t => t.UserId == userId).Select(t => new { t.TimelineId }).ToListAsync();

            List<TimelineInfo> result = new();

            foreach (var entity in entities)
            {
                result.Add(await _timelineService.GetTimelineById(entity.TimelineId));
            }

            return result;
        }

        public async Task MoveBookmark(long userId, string timelineName, long newPosition)
        {
            if (timelineName == null)
                throw new ArgumentNullException(nameof(timelineName));

            var timelineId = await _timelineService.GetTimelineIdByName(timelineName);

            var entity = await _database.BookmarkTimelines.SingleOrDefaultAsync(t => t.TimelineId == timelineId && t.UserId == userId);

            if (entity == null) throw new InvalidBookmarkException("You can't move a non-bookmark timeline.");

            var oldPosition = entity.Rank;

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

        public async Task<bool> RemoveBookmark(long userId, string timelineName)
        {
            if (timelineName is null)
                throw new ArgumentNullException(nameof(timelineName));

            if (!await _userService.CheckUserExistence(userId))
                throw new UserNotExistException(userId);

            var timelineId = await _timelineService.GetTimelineIdByName(timelineName);

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
