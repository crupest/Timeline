﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Services;

namespace Timeline.Models.Mapper
{
    public class TimelineMapper
    {
        private readonly DatabaseContext _database;
        private readonly UserMapper _userMapper;
        private readonly IHighlightTimelineService _highlightTimelineService;
        private readonly IBookmarkTimelineService _bookmarkTimelineService;

        public TimelineMapper(DatabaseContext database, UserMapper userMapper, IHighlightTimelineService highlightTimelineService, IBookmarkTimelineService bookmarkTimelineService)
        {
            _database = database;
            _userMapper = userMapper;
            _highlightTimelineService = highlightTimelineService;
            _bookmarkTimelineService = bookmarkTimelineService;
        }

        public async Task<HttpTimeline> MapToHttp(TimelineEntity entity, IUrlHelper urlHelper, long? userId)
        {
            await _database.Entry(entity).Reference(e => e.Owner).LoadAsync();
            await _database.Entry(entity).Collection(e => e.Members).Query().Include(m => m.User).LoadAsync();

            var timelineName = entity.Name is null ? "@" + entity.Owner.Username : entity.Name;

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
                links: new HttpTimelineLinks(
                    self: urlHelper.ActionLink(nameof(TimelineController.TimelineGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { timeline = timelineName }),
                    posts: urlHelper.ActionLink(nameof(TimelinePostController.List), nameof(TimelinePostController)[0..^nameof(Controller).Length], new { timeline = timelineName })
                )
            );
        }

        public async Task<List<HttpTimeline>> MapToHttp(List<TimelineEntity> entities, IUrlHelper urlHelper, long? userId)
        {
            var result = new List<HttpTimeline>();
            foreach (var entity in entities)
            {
                result.Add(await MapToHttp(entity, urlHelper, userId));
            }
            return result;
        }


        public async Task<HttpTimelinePost> MapToHttp(TimelinePostEntity entity, string timelineName, IUrlHelper urlHelper)
        {
            await _database.Entry(entity).Collection(p => p.DataList).LoadAsync();
            await _database.Entry(entity).Reference(e => e.Author).LoadAsync();

            List<HttpTimelinePostDataDigest> dataDigestList = entity.DataList.OrderBy(d => d.Index).Select(d => new HttpTimelinePostDataDigest(d.Kind, d.DataTag, d.LastUpdated)).ToList();

            HttpUser? author = null;
            if (entity.Author is not null)
            {
                author = await _userMapper.MapToHttp(entity.Author, urlHelper);
            }

            return new HttpTimelinePost(
                    id: entity.LocalId,
                    dataList: dataDigestList,
                    time: entity.Time,
                    author: author,
                    color: entity.Color,
                    deleted: entity.Deleted,
                    lastUpdated: entity.LastUpdated
                );
        }

        public async Task<List<HttpTimelinePost>> MapToHttp(List<TimelinePostEntity> entities, string timelineName, IUrlHelper urlHelper)
        {
            var result = new List<HttpTimelinePost>();
            foreach (var entity in entities)
            {
                result.Add(await MapToHttp(entity, timelineName, urlHelper));
            }
            return result;
        }
    }
}
