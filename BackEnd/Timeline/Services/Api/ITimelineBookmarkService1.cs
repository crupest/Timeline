using System;
using System.Threading.Tasks;
using Timeline.Models;

namespace Timeline.Services.Api
{
    public interface ITimelineBookmarkService1
    {
        Task<Page<TimelineBookmark>> GetBookmarksAsync(long userId, int page, int pageSize);

        Task<TimelineBookmark> GetBookmarkAsync(long userId, long timelineId);

        Task<TimelineBookmark> AddBookmarkAsync(long userId, long timelineId, int? position = null);

        Task DeleteBookmarkAsync(long userId, long timelineId);

        Task<TimelineBookmark> MoveBookmarkAsync(long userId, long timelineId, int position);

        Task<TimelineVisibility> GetBookmarkVisibilityAsync(long userId);

        Task SetBookmarkVisibilityAsync(long userId, TimelineVisibility visibility);

        Task<bool> CanReadBookmarksAsync(long userId, long? visitorId);
    }
}

