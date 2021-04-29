using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Services.Api
{

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

        public async Task<bool> AddBookmarkAsync(long userId, long timelineId)
        {
            if (!await _userService.CheckUserExistenceAsync(userId))
                throw new UserNotExistException(userId);

            if (!await _timelineService.CheckTimelineExistenceAsync(timelineId))
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

        public async Task<List<long>> GetBookmarksAsync(long userId)
        {
            if (!await _userService.CheckUserExistenceAsync(userId))
                throw new UserNotExistException(userId);

            var entities = await _database.BookmarkTimelines.Where(t => t.UserId == userId).OrderBy(t => t.Rank).Select(t => new { t.TimelineId }).ToListAsync();

            return entities.Select(e => e.TimelineId).ToList();
        }

        public async Task<bool> IsBookmarkAsync(long userId, long timelineId, bool checkUserExistence = true, bool checkTimelineExistence = true)
        {
            if (checkUserExistence && !await _userService.CheckUserExistenceAsync(userId))
                throw new UserNotExistException(userId);

            if (checkTimelineExistence && !await _timelineService.CheckTimelineExistenceAsync(timelineId))
                throw new TimelineNotExistException(timelineId);

            return await _database.BookmarkTimelines.AnyAsync(b => b.TimelineId == timelineId && b.UserId == userId);
        }

        public async Task MoveBookmarkAsync(long userId, long timelineId, long newPosition)
        {
            if (!await _userService.CheckUserExistenceAsync(userId))
                throw new UserNotExistException(userId);

            if (!await _timelineService.CheckTimelineExistenceAsync(timelineId))
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

        public async Task<bool> RemoveBookmarkAsync(long userId, long timelineId)
        {
            if (!await _userService.CheckUserExistenceAsync(userId))
                throw new UserNotExistException(userId);

            if (!await _timelineService.CheckTimelineExistenceAsync(timelineId))
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
