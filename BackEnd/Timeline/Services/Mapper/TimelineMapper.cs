using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Controllers;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Services.Api;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.Services.Mapper
{
    public class TimelineMapper : IMapper<TimelineEntity, HttpTimeline>,
        IMapper<TimelinePostEntity, HttpTimelinePost>
    {
        private readonly DatabaseContext _database;
        private readonly IMapper<UserEntity, HttpUser> _userMapper;
        private readonly IHighlightTimelineService _highlightTimelineService;
        private readonly IBookmarkTimelineService _bookmarkTimelineService;
        private readonly ITimelineService _timelineService;
        private readonly ITimelinePostService _timelinePostService;

        public TimelineMapper(DatabaseContext database, IMapper<UserEntity, HttpUser> userMapper, IHighlightTimelineService highlightTimelineService, IBookmarkTimelineService bookmarkTimelineService, ITimelineService timelineService, ITimelinePostService timelinePostService)
        {
            _database = database;
            _userMapper = userMapper;
            _highlightTimelineService = highlightTimelineService;
            _bookmarkTimelineService = bookmarkTimelineService;
            _timelineService = timelineService;
            _timelinePostService = timelinePostService;
        }

        private string CalculateTimelineName(TimelineEntity entity)
        {
            return entity.Name is null ? "@" + entity.Owner.Username : entity.Name;
        }

        public async Task<HttpTimeline> MapAsync(TimelineEntity entity, IUrlHelper urlHelper, ClaimsPrincipal? user)
        {
            var userId = user.GetOptionalUserId();

            await _database.Entry(entity).Reference(e => e.Owner).LoadAsync();
            await _database.Entry(entity).Collection(e => e.Members).Query().Include(m => m.User).LoadAsync();

            var timelineName = CalculateTimelineName(entity);

            bool manageable;

            if (user is null || userId is null)
            {
                manageable = false;
            }
            else if (user.HasPermission(UserPermission.AllTimelineManagement))
            {
                manageable = true;
            }
            else
            {
                manageable = await _timelineService.HasManagePermissionAsync(entity.Id, userId.Value);
            }

            bool postable;
            if (user is null || userId is null)
            {
                postable = false;
            }
            else
            {
                postable = await _timelineService.IsMemberOfAsync(entity.Id, userId.Value);
            }

            var nameV2 = entity.Name is null ? "self" : entity.Name;
            var ownerUsername = entity.Owner.Username;

            return new HttpTimeline(
                uniqueId: entity.UniqueId,
                title: string.IsNullOrEmpty(entity.Title) ? timelineName : entity.Title,
                name: timelineName,
                nameV2: nameV2,
                nameLastModifed: entity.NameLastModified,
                description: entity.Description ?? "",
                owner: await _userMapper.MapAsync(entity.Owner, urlHelper, user),
                visibility: entity.Visibility,
                members: await _userMapper.MapListAsync(entity.Members.Select(m => m.User).ToList(), urlHelper, user),
                color: entity.Color,
                createTime: entity.CreateTime,
                lastModified: entity.LastModified,
                isHighlight: await _highlightTimelineService.IsHighlightTimelineAsync(entity.Id),
                isBookmark: userId is not null && await _bookmarkTimelineService.IsBookmarkAsync(userId.Value, entity.Id, false, false),
                manageable: manageable,
                postable: postable,
                links: new HttpTimelineLinks(
                    self: urlHelper.ActionLink("Get", "TimelineV2", new { owner = ownerUsername, timeline = nameV2 }) ?? throw Exception("Failed to generate link for timeline self."),
                    posts: urlHelper.ActionLink("List", "TimelinePostV2", new { owner = ownerUsername, timeline = nameV2 }) ?? throw Exception("Failed to generate link for timeline posts.")
                )
            );
        }

        private System.Exception Exception(string v)
        {
            throw new System.NotImplementedException();
        }

        public async Task<HttpTimelinePost> MapAsync(TimelinePostEntity entity, IUrlHelper urlHelper, ClaimsPrincipal? user)
        {
            var userId = user.GetOptionalUserId();

            await _database.Entry(entity).Reference(e => e.Timeline).LoadAsync();
            await _database.Entry(entity).Collection(p => p.DataList).LoadAsync();
            await _database.Entry(entity).Reference(e => e.Author).LoadAsync();
            await _database.Entry(entity.Timeline).Reference(e => e.Owner).LoadAsync();

            List<HttpTimelinePostDataDigest> dataDigestList = entity.DataList.OrderBy(d => d.Index).Select(d => new HttpTimelinePostDataDigest(d.Kind, $"\"{d.DataTag}\"", d.LastUpdated)).ToList();

            HttpUser? author = null;
            if (entity.Author is not null)
            {
                author = await _userMapper.MapAsync(entity.Author, urlHelper, user);
            }

            bool editable;

            if (user is null || userId is null)
            {
                editable = false;
            }
            else if (user.HasPermission(UserPermission.AllTimelineManagement))
            {
                editable = true;
            }
            else
            {
                editable = await _timelinePostService.HasPostModifyPermissionAsync(entity.TimelineId, entity.LocalId, userId.Value);
            }

            return new HttpTimelinePost(
                    id: entity.LocalId,
                    dataList: dataDigestList,
                    time: entity.Time,
                    author: author,
                    color: entity.Color,
                    deleted: entity.Deleted,
                    lastUpdated: entity.LastUpdated,
                    timelineOwnerV2: entity.Timeline.Owner.Username,
                    timelineNameV2: entity.Timeline.Name is null ? "self" : entity.Timeline.Name,
                    timelineName: CalculateTimelineName(entity.Timeline),
                    editable: editable
                );
        }
    }
}
