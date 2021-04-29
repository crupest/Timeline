using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Services.Api
{
    public class HighlightTimelineService : IHighlightTimelineService
    {
        private readonly DatabaseContext _database;
        private readonly IBasicUserService _userService;
        private readonly IBasicTimelineService _timelineService;
        private readonly IClock _clock;

        public HighlightTimelineService(DatabaseContext database, IBasicUserService userService, IBasicTimelineService timelineService, IClock clock)
        {
            _database = database;
            _userService = userService;
            _timelineService = timelineService;
            _clock = clock;
        }

        public async Task<bool> AddHighlightTimelineAsync(long timelineId, long? operatorId)
        {
            if (!await _timelineService.CheckTimelineExistenceAsync(timelineId))
                throw new TimelineNotExistException(timelineId);

            if (operatorId.HasValue && !await _userService.CheckUserExistenceAsync(operatorId.Value))
            {
                throw new UserNotExistException(null, operatorId.Value, "User with given operator id does not exist.", null);
            }

            var alreadyIs = await _database.HighlightTimelines.AnyAsync(t => t.TimelineId == timelineId);

            if (alreadyIs) return false;

            _database.HighlightTimelines.Add(new HighlightTimelineEntity { TimelineId = timelineId, OperatorId = operatorId, AddTime = _clock.GetCurrentTime(), Order = await _database.HighlightTimelines.CountAsync() + 1 });
            await _database.SaveChangesAsync();
            return true;
        }

        public async Task<List<long>> GetHighlightTimelinesAsync()
        {
            var entities = await _database.HighlightTimelines.OrderBy(t => t.Order).Select(t => new { t.TimelineId }).ToListAsync();

            return entities.Select(e => e.TimelineId).ToList();
        }

        public async Task<bool> RemoveHighlightTimelineAsync(long timelineId, long? operatorId)
        {
            if (!await _timelineService.CheckTimelineExistenceAsync(timelineId))
                throw new TimelineNotExistException(timelineId);

            if (operatorId.HasValue && !await _userService.CheckUserExistenceAsync(operatorId.Value))
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

        public async Task MoveHighlightTimelineAsync(long timelineId, long newPosition)
        {
            if (!await _timelineService.CheckTimelineExistenceAsync(timelineId))
                throw new TimelineNotExistException(timelineId);

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

        public async Task<bool> IsHighlightTimelineAsync(long timelineId, bool checkTimelineExistence = true)
        {
            if (checkTimelineExistence && !await _timelineService.CheckTimelineExistenceAsync(timelineId))
                throw new TimelineNotExistException(timelineId);

            return await _database.HighlightTimelines.AnyAsync(t => t.TimelineId == timelineId);
        }
    }
}
