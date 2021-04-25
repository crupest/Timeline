using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Services.Api;
using Timeline.Services.Timeline;

namespace Timeline.Services.Mapper
{
    public class TimelineMapper
    {
        private readonly DatabaseContext _database;
        private readonly UserMapper _userMapper;
        private readonly IHighlightTimelineService _highlightTimelineService;
        private readonly IBookmarkTimelineService _bookmarkTimelineService;
        private readonly ITimelineService _timelineService;
        private readonly ITimelinePostService _timelinePostService;

        public TimelineMapper(DatabaseContext database, UserMapper userMapper, IHighlightTimelineService highlightTimelineService, IBookmarkTimelineService bookmarkTimelineService, ITimelineService timelineService, ITimelinePostService timelinePostService)
        {
            _database = database;
            _userMapper = userMapper;
            _highlightTimelineService = highlightTimelineService;
            _bookmarkTimelineService = bookmarkTimelineService;
            _timelineService = timelineService;
            _timelinePostService = timelinePostService;
        }

        public async Task<HttpTimeline> MapToHttp(TimelineEntity entity, IUrlHelper urlHelper, long? userId, bool isAdministrator)
        {
            await _database.Entry(entity).Reference(e => e.Owner).LoadAsync();
            await _database.Entry(entity).Collection(e => e.Members).Query().Include(m => m.User).LoadAsync();

            var timelineName = entity.Name is null ? "@" + entity.Owner.Username : entity.Name;

            bool manageable;

            if (userId is null)
            {
                manageable = false;
            }
            else if (isAdministrator)
            {
                manageable = true;
            }
            else
            {
                manageable = await _timelineService.HasManagePermission(entity.Id, userId.Value);
            }

            bool postable;
            if (userId is null)
            {
                postable = false;
            }
            else
            {
                postable = await _timelineService.IsMemberOf(entity.Id, userId.Value);
            }

            return new HttpTimeline(
                uniqueId: entity.UniqueId,
                title: string.IsNullOrEmpty(entity.Title) ? timelineName : entity.Title,
                name: timelineName,
                nameLastModifed: entity.NameLastModified,
                description: entity.Description ?? "",
                owner: await _userMapper.MapToHttp(entity.Owner, urlHelper),
                visibility: entity.Visibility,
                members: await _userMapper.MapToHttp(entity.Members.Select(m => m.User).ToList(), urlHelper),
                color: entity.Color,
                createTime: entity.CreateTime,
                lastModified: entity.LastModified,
                isHighlight: await _highlightTimelineService.IsHighlightTimeline(entity.Id),
                isBookmark: userId is not null && await _bookmarkTimelineService.IsBookmark(userId.Value, entity.Id, false, false),
                manageable: manageable,
                postable: postable,
                links: new HttpTimelineLinks(
                    self: urlHelper.ActionLink(nameof(TimelineController.TimelineGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { timeline = timelineName }),
                    posts: urlHelper.ActionLink(nameof(TimelinePostController.List), nameof(TimelinePostController)[0..^nameof(Controller).Length], new { timeline = timelineName })
                )
            );
        }

        public async Task<List<HttpTimeline>> MapToHttp(List<TimelineEntity> entities, IUrlHelper urlHelper, long? userId, bool isAdministrator)
        {
            var result = new List<HttpTimeline>();
            foreach (var entity in entities)
            {
                result.Add(await MapToHttp(entity, urlHelper, userId, isAdministrator));
            }
            return result;
        }


        public async Task<HttpTimelinePost> MapToHttp(TimelinePostEntity entity, string timelineName, IUrlHelper urlHelper, long? userId, bool isAdministrator)
        {
            _ = timelineName;

            await _database.Entry(entity).Collection(p => p.DataList).LoadAsync();
            await _database.Entry(entity).Reference(e => e.Author).LoadAsync();

            List<HttpTimelinePostDataDigest> dataDigestList = entity.DataList.OrderBy(d => d.Index).Select(d => new HttpTimelinePostDataDigest(d.Kind, $"\"{d.DataTag}\"", d.LastUpdated)).ToList();

            HttpUser? author = null;
            if (entity.Author is not null)
            {
                author = await _userMapper.MapToHttp(entity.Author, urlHelper);
            }

            bool editable;

            if (userId is null)
            {
                editable = false;
            }
            else if (isAdministrator)
            {
                editable = true;
            }
            else
            {
                editable = await _timelinePostService.HasPostModifyPermission(entity.TimelineId, entity.LocalId, userId.Value);
            }


            return new HttpTimelinePost(
                    id: entity.LocalId,
                    dataList: dataDigestList,
                    time: entity.Time,
                    author: author,
                    color: entity.Color,
                    deleted: entity.Deleted,
                    lastUpdated: entity.LastUpdated,
                    timelineName: timelineName,
                    editable: editable
                );
        }

        public async Task<List<HttpTimelinePost>> MapToHttp(List<TimelinePostEntity> entities, string timelineName, IUrlHelper urlHelper, long? userId, bool isAdministrator)
        {
            var result = new List<HttpTimelinePost>();
            foreach (var entity in entities)
            {
                result.Add(await MapToHttp(entity, timelineName, urlHelper, userId, isAdministrator));
            }
            return result;
        }

        internal Task MapToHttp(TimelinePostEntity post, string timeline, IUrlHelper url)
        {
            throw new System.NotImplementedException();
        }
    }
}
