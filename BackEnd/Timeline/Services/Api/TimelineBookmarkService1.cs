using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Services.Api
{
    public class TimelineBookmarkService1 : ITimelineBookmarkService1
    {
        private DatabaseContext _databaseContext;
        private IUserService _userService;
        private ITimelineService _timelineService;

        public TimelineBookmarkService1(DatabaseContext databaseContext, IUserService userService, ITimelineService timelineService)
        {
            _databaseContext = databaseContext;
            _userService = userService;
            _timelineService = timelineService;

        }

        public async Task<TimelineBookmark> AddBookmarkAsync(long userId, long timelineId, int? position = null)
        {
            var user = await _userService.GetUserAsync(userId);
            var timeline = await _timelineService.GetTimelineAsync(timelineId);

            if (await _databaseContext.BookmarkTimelines.AnyAsync(b => b.UserId == userId && b.TimelineId == timelineId))
            {
                throw new EntityAlreadyExistException(EntityTypes.BookmarkTimeline, new Dictionary<string, object>
                {
                    {"user-id", userId },
                    {"timeline-id", timelineId }
                });
            }

            var count = await _databaseContext.BookmarkTimelines.Where(b => b.UserId == userId).CountAsync();

            if (position.HasValue)
            {
                if (position <= 0)
                {
                    position = 1;
                }
                else if (position > count + 1)
                {
                    position = count + 1;
                }
            }
            else
            {
                position = count + 1;
            }

            await using var transaction = await _databaseContext.Database.BeginTransactionAsync();

            if (position.Value < count + 1)
            {
                await _databaseContext.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = `rank` + 1 WHERE `rank` >= {0} AND `user` = {1}", position.Value, userId);
            }

            BookmarkTimelineEntity entity = new BookmarkTimelineEntity
            {
                UserId = userId,
                TimelineId = timelineId,
                Rank = position.Value
            };

            _databaseContext.BookmarkTimelines.Add(entity);
            await _databaseContext.SaveChangesAsync();

            await _databaseContext.Database.CommitTransactionAsync();

            return new TimelineBookmark(user.Username, timeline.Name is null ? "self" : timeline.Name, position.Value);
        }

        public async Task<bool> CanReadBookmarksAsync(long userId, long? visitorId)
        {
            var visibility = await GetBookmarkVisibilityAsync(userId);
            if (visibility == TimelineVisibility.Public) return true;
            else if (visibility == TimelineVisibility.Register) return visitorId is not null;
            else return userId == visitorId;
        }

        public async Task DeleteBookmarkAsync(long userId, long timelineId)
        {
            var entity = await _databaseContext.BookmarkTimelines.Where(b => b.UserId == userId && b.TimelineId == timelineId).SingleOrDefaultAsync();
            if (entity is null) return;

            await _databaseContext.Database.BeginTransactionAsync();
            await _databaseContext.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = `rank` - 1 WHERE `rank` > {0} AND `user` = {1}", entity.Rank, userId);
            _databaseContext.BookmarkTimelines.Remove(entity);
            await _databaseContext.SaveChangesAsync();
            await _databaseContext.Database.CommitTransactionAsync();
        }

        public async Task<TimelineBookmark> GetBookmarkAsync(long userId, long timelineId)
        {
            var user = await _userService.GetUserAsync(userId);
            var timeline = await _timelineService.GetTimelineAsync(timelineId);
            var entity = await _databaseContext.BookmarkTimelines.Where(b => b.UserId == userId && b.TimelineId == timelineId).SingleOrDefaultAsync();

            if (entity is null)
            {
                throw new EntityNotExistException(EntityTypes.BookmarkTimeline, new Dictionary<string, object>
                {
                    { "user-id", userId },
                    { "timeline-id", timelineId }
                });
            }

            return new TimelineBookmark(user.Username, timeline.Name is null ? "self" : timeline.Name, (int)entity.Rank);
        }

        public async Task<TimelineBookmark> GetBookmarkAtAsync(long userId, int position)
        {
            var user = await _userService.GetUserAsync(userId);
            var entity = await _databaseContext.BookmarkTimelines.Where(b => b.UserId == userId && b.Rank == position).SingleOrDefaultAsync();

            if (entity is null)
            {
                throw new EntityNotExistException(EntityTypes.BookmarkTimeline, new Dictionary<string, object>
                {
                    { "user-id", userId },
                    { "position", position }
                });
            }

            var timeline = await _timelineService.GetTimelineAsync(entity.TimelineId);

            return new TimelineBookmark(user.Username, timeline.Name is null ? "self" : timeline.Name, (int)entity.Rank);
        }

        public async Task<Page<TimelineBookmark>> GetBookmarksAsync(long userId, int page, int pageSize)
        {
            if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var user = await _userService.GetUserAsync(userId);

            var totalCount = await _databaseContext.BookmarkTimelines.Where(b => b.UserId == userId).CountAsync();
            var entities = await _databaseContext.BookmarkTimelines.Where(b => b.UserId == userId).Skip(pageSize * (page - 1)).Take(pageSize).ToListAsync();

            var items = new List<TimelineBookmark>();
            foreach (var entity in entities)
            {
                var timeline = await _timelineService.GetTimelineAsync(entity.TimelineId);
                items.Add(new TimelineBookmark(user.Username, timeline.Name is null ? "self" : timeline.Name, (int)entity.Rank));
            }

            return new Page<TimelineBookmark>(page, pageSize, totalCount, items);
        }

        public async Task<TimelineVisibility> GetBookmarkVisibilityAsync(long userId)
        {
            await _userService.CheckUserExistenceAsync(userId);
            var configEntity = await _databaseContext.UserConfigurations.Where(c => c.UserId == userId).SingleOrDefaultAsync();
            if (configEntity is null) return TimelineVisibility.Private;
            return configEntity.BookmarkVisibility;
        }

        public async Task<TimelineBookmark> MoveBookmarkAsync(long userId, long timelineId, int position)
        {
            var user = await _userService.GetUserAsync(userId);
            var timeline = await _timelineService.GetTimelineAsync(timelineId);

            var entity = await _databaseContext.BookmarkTimelines.SingleOrDefaultAsync(t => t.TimelineId == timelineId && t.UserId == userId);

            if (entity is null)
            {
                throw new EntityNotExistException(EntityTypes.BookmarkTimeline);
            }

            var oldPosition = entity.Rank;

            if (position < 1)
            {
                position = 1;
            }
            else
            {
                var totalCount = await _databaseContext.BookmarkTimelines.CountAsync(t => t.UserId == userId);
                if (position > totalCount) position = totalCount;
            }

            if (oldPosition != position)
            {
                await using var transaction = await _databaseContext.Database.BeginTransactionAsync();

                if (position > oldPosition)
                {
                    await _databaseContext.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = `rank` - 1 WHERE `rank` BETWEEN {0} AND {1} AND `user` = {2}", oldPosition + 1, position, userId);
                    await _databaseContext.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = {0} WHERE `id` = {1}", position, entity.Id);
                }
                else
                {
                    await _databaseContext.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = `rank` + 1 WHERE `rank` BETWEEN {0} AND {1} AND `user` = {2}", position, oldPosition - 1, userId);
                    await _databaseContext.Database.ExecuteSqlRawAsync("UPDATE `bookmark_timelines` SET `rank` = {0} WHERE `id` = {1}", position, entity.Id);
                }

                await transaction.CommitAsync();
            }

            return new TimelineBookmark(user.Username, timeline.Name is null ? "self" : timeline.Name, (int)entity.Rank);
        }

        public async Task SetBookmarkVisibilityAsync(long userId, TimelineVisibility visibility)
        {
            await _userService.CheckUserExistenceAsync(userId);
            var configEntity = await _databaseContext.UserConfigurations.Where(c => c.UserId == userId).SingleOrDefaultAsync();
            if (configEntity is null)
            {
                configEntity = new UserConfigurationEntity
                {
                    UserId = userId,
                    BookmarkVisibility = visibility
                };
                _databaseContext.UserConfigurations.Add(configEntity);
            }
            else
            {
                configEntity.BookmarkVisibility = visibility;
            }

            await _databaseContext.SaveChangesAsync();
        }
    }
}
