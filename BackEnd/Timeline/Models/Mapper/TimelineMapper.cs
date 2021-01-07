using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Controllers;
using Timeline.Entities;
using Timeline.Models.Http;
using Timeline.Services;

namespace Timeline.Models.Mapper
{
    public static class TimelineMapper
    {
        public static HttpTimeline MapToHttp(this TimelineEntity entity, IUrlHelper urlHelper)
        {
            var timelineName = entity.Name is null ? "@" + entity.Owner.Username : entity.Name;

            return new HttpTimeline(
                uniqueId: entity.UniqueId,
                title: string.IsNullOrEmpty(entity.Title) ? timelineName : entity.Title,
                name: timelineName,
                nameLastModifed: entity.NameLastModified,
                description: entity.Description ?? "",
                owner: entity.Owner.MapToHttp(urlHelper),
                visibility: entity.Visibility,
                members: entity.Members.Select(m => m.User.MapToHttp(urlHelper)).ToList(),
                createTime: entity.CreateTime,
                lastModified: entity.LastModified,
                links: new HttpTimelineLinks(
                    self: urlHelper.ActionLink(nameof(TimelineController.TimelineGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { timeline = timelineName }),
                    posts: urlHelper.ActionLink(nameof(TimelineController.PostListGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { timeline = timelineName })
                )
            );
        }

        public static List<HttpTimeline> MapToHttp(this List<TimelineEntity> entites, IUrlHelper urlHelper)
        {
            return entites.Select(e => e.MapToHttp(urlHelper)).ToList();
        }


        public static HttpTimelinePost MapToHttp(this TimelinePostEntity entity, string timelineName, IUrlHelper urlHelper)
        {
            HttpTimelinePostContent? content = null;

            if (entity.Content != null)
            {
                content = entity.ContentType switch
                {
                    TimelinePostContentTypes.Text => new HttpTimelinePostContent
                    (
                        type: TimelinePostContentTypes.Text,
                        text: entity.Content,
                        url: null,
                        eTag: null
                    ),
                    TimelinePostContentTypes.Image => new HttpTimelinePostContent
                    (
                        type: TimelinePostContentTypes.Image,
                        text: null,
                        url: urlHelper.ActionLink(nameof(TimelineController.PostDataGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { timeline = timelineName, post = entity.LocalId }),
                        eTag: $"\"{entity.Content}\""
                    ),
                    _ => throw new DatabaseCorruptedException(string.Format(CultureInfo.InvariantCulture, "Unknown timeline post type {0}.", entity.ContentType))
                };
            }

            return new HttpTimelinePost(
                    id: entity.LocalId,
                    content: content,
                    deleted: content is null,
                    time: entity.Time,
                    author: entity.Author?.MapToHttp(urlHelper),
                    lastUpdated: entity.LastUpdated
                );
        }

        public static List<HttpTimelinePost> MapToHttp(this List<TimelinePostEntity> entities, string timelineName, IUrlHelper urlHelper)
        {
            return entities.Select(e => e.MapToHttp(timelineName, urlHelper)).ToList();
        }
    }
}
