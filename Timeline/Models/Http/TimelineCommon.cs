using System;
using System.Collections.Generic;

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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is a DTO class.")]
    public class BaseTimelineInfo
    {
        public string Description { get; set; } = default!;
        public UserInfo Owner { get; set; } = default!;
        public TimelineVisibility Visibility { get; set; }
        public List<UserInfo> Members { get; set; } = default!;
    }

    public class TimelineInfo : BaseTimelineInfo
    {
        public string Name { get; set; } = default!;
    }
}
