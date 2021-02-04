using System;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Info of a post.
    /// </summary>
    public class HttpTimelinePost
    {
        public HttpTimelinePost() { }

        public HttpTimelinePost(long id, HttpTimelinePostContent? content, bool deleted, DateTime time, HttpUser? author, string? color, DateTime lastUpdated)
        {
            Id = id;
            Content = content;
            Deleted = deleted;
            Time = time;
            Author = author;
            Color = color;
            LastUpdated = lastUpdated;
        }

        /// <summary>
        /// Post id.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Content of the post. May be null if post is deleted.
        /// </summary>
        public HttpTimelinePostContent? Content { get; set; }
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
    }
}
