using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Timeline.Controllers;

namespace Timeline.Models.Http
{
    public enum TimelineVisibility
    {
        /// <summary>
        /// All people including those without accounts.
        /// </summary>
        Public,
        /// <summary>
        /// Only people signed in.
        /// </summary>
        Register,
        /// <summary>
        /// Only member.
        /// </summary>
        Private
    }

    public static class TimelinePostContentTypes
    {
        public const string Text = "text";
        public const string Image = "image";
    }

    public interface ITimelinePostContent
    {
        public string Type { get; }
    }

    public class TextTimelinePostContent : ITimelinePostContent
    {
        public TextTimelinePostContent() { }
        public TextTimelinePostContent(string text) { Text = text; }

        public string Type { get; } = TimelinePostContentTypes.Text;
        public string Text { get; set; } = "";
    }

    public class ImageTimelinePostContent : ITimelinePostContent
    {
        public ImageTimelinePostContent() { }
        public ImageTimelinePostContent(string dataTag) { DataTag = dataTag; }

        public string Type { get; } = TimelinePostContentTypes.Image;
        [JsonIgnore]
        public string DataTag { get; set; } = "";
        public string? Url { get; set; } = null;
    }

    public class TimelinePostInfo
    {
        public long Id { get; set; }
        // use object to make json serializer use the runtime type
        public object Content { get; set; } = default!;
        public DateTime Time { get; set; }
        public UserInfo Author { get; set; } = default!;
        public DateTime LastUpdated { get; set; } = default!;
    }

    public class TimelineInfo
    {
        public string? Name { get; set; }
        public string Description { get; set; } = default!;
        public UserInfo Owner { get; set; } = default!;
        public TimelineVisibility Visibility { get; set; }
#pragma warning disable CA2227 // Collection properties should be read only
        public List<UserInfo> Members { get; set; } = default!;
#pragma warning restore CA2227 // Collection properties should be read only

#pragma warning disable CA1707 // Identifiers should not contain underscores
        public TimelineInfoLinks? _links { get; set; }
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }

    public class TimelineInfoLinks
    {
        public string Self { get; set; } = default!;
        public string Posts { get; set; } = default!;
    }

    public static class TimelineInfoExtensions
    {
        public static TimelineInfo FillLinks(this TimelineInfo info, IUrlHelper urlHelper)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (urlHelper == null)
                throw new ArgumentNullException(nameof(urlHelper));

            if (string.IsNullOrEmpty(info.Name))
            {
                info._links = new TimelineInfoLinks
                {
                    Self = urlHelper.ActionLink(nameof(PersonalTimelineController.TimelineGet), nameof(PersonalTimelineController)[0..^nameof(Controller).Length], new { info.Owner.Username }),
                    Posts = urlHelper.ActionLink(nameof(PersonalTimelineController.PostListGet), nameof(PersonalTimelineController)[0..^nameof(Controller).Length], new { info.Owner.Username })
                };
            }
            else
            {
                info._links = new TimelineInfoLinks
                {
                    Self = urlHelper.ActionLink(nameof(TimelineController.TimelineGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { info.Name }),
                    Posts = urlHelper.ActionLink(nameof(TimelineController.PostListGet), nameof(TimelineController)[0..^nameof(Controller).Length], new { info.Name })
                };
            }

            return info;
        }
    }
}
