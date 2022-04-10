using System;
using System.Collections.Generic;

namespace Timeline.Models.Http
{

    /// <summary>
    /// Info of a post.
    /// </summary>
    public class HttpTimelinePost
    {
        public HttpTimelinePost() { }

        public HttpTimelinePost(long id, List<HttpTimelinePostDataDigest> dataList, bool deleted, DateTime time, HttpUser? author, string? color, DateTime lastUpdated, string timelineOwnerV2, string timelineNameV2, string timelineName, bool editable)
        {
            Id = id;
            DataList = dataList;
            Deleted = deleted;
            Time = time;
            Author = author;
            Color = color;
            LastUpdated = lastUpdated;
            TimelineOwnerV2 = timelineOwnerV2;
            TimelineNameV2 = timelineNameV2;
            TimelineName = timelineName;
            Editable = editable;
        }

        /// <summary>
        /// Post id.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// The data list.
        /// </summary>
#pragma warning disable CA2227
        public List<HttpTimelinePostDataDigest> DataList { get; set; } = default!;
#pragma warning restore CA2227
        /// <summary>
        /// True if post is deleted.
        /// </summary>
        public bool Deleted { get; set; }
        /// <summary>
        /// Post time.
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// The author. May be null if the user has been deleted.
        /// </summary>
        public HttpUser? Author { get; set; } = default!;
        /// <summary>
        /// The color.
        /// </summary>
        public string? Color { get; set; }
        /// <summary>
        /// Last updated time.
        /// </summary>
        public DateTime LastUpdated { get; set; } = default!;
        /// <summary>
        /// Timeline owner username.
        /// </summary>
        public string TimelineOwnerV2 { get; set; } = default!;
        /// <summary>
        /// Timeline name.
        /// </summary>
        public string TimelineNameV2 { get; set; } = default!;
        /// <summary>
        /// Timeline name.
        /// </summary>
        public string TimelineName { get; set; } = default!;
        /// <summary>
        /// True if you can edit this post.
        /// </summary>
        public bool Editable { get; set; }
    }
}
