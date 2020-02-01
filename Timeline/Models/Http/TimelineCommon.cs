using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

    public class TimelinePostInfo
    {
        public long Id { get; set; }
        public string Content { get; set; } = default!;
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

            info._links = new TimelineInfoLinks
            {
                Posts = urlHelper.ActionLink(nameof(PersonalTimelineController.PostListGet), nameof(PersonalTimelineController)[0..^nameof(Controller).Length], new { info.Owner.Username })
            };

            return info;
        }
    }
}
