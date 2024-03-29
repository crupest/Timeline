﻿using Microsoft.EntityFrameworkCore;
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
        private readonly IUserService _userService;
        private readonly ITimelineService _timelineService;
        private readonly IClock _clock;

        public HighlightTimelineService(DatabaseContext database, IUserService userService, ITimelineService timelineService, IClock clock)
        {
            _database = database;
            _userService = userService;
            _timelineService = timelineService;
            _clock = clock;
        }

        public async Task<bool> AddHighlightTimelineAsync(long timelineId, long? operatorId)
        {
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            if (operatorId.HasValue)
            {
                await _userService.ThrowIfUserNotExist(operatorId.Value);
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
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            if (operatorId.HasValue)
            {
                await _userService.ThrowIfUserNotExist(operatorId.Value);
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
            await _timelineService.ThrowIfTimelineNotExist(timelineId);

            var entity = await _database.HighlightTimelines.SingleOrDefaultAsync(t => t.TimelineId == timelineId);

            if (entity is null)
            {
                throw new EntityNotExistException(EntityTypes.HighlightTimeline);
            }

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
            if (checkTimelineExistence)
                await _timelineService.ThrowIfTimelineNotExist(timelineId);

            return await _database.HighlightTimelines.AnyAsync(t => t.TimelineId == timelineId);
        }
    }
}
