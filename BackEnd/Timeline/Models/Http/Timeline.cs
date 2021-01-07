using System;
using System.Collections.Generic;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Info of post content.
    /// </summary>
    public class HttpTimelinePostContent
    {
        public HttpTimelinePostContent() { }

        public HttpTimelinePostContent(string type, string? text, string? url, string? eTag)
        {
            Type = type;
            Text = text;
            Url = url;
            ETag = eTag;
        }

        /// <summary>
        /// Type of the post content.
        /// </summary>
        public string Type { get; set; } = default!;
        /// <summary>
        /// If post is of text type. This is the text.
        /// </summary>
        public string? Text { get; set; }
        /// <summary>
        /// If post is of image type. This is the image url.
        /// </summary>
        public string? Url { get; set; }
        /// <summary>
        /// If post has data (currently it means it's a image post), this is the data etag.
        /// </summary>
        public string? ETag { get; set; }
    }

    /// <summary>
    /// Info of a post.
    /// </summary>
    public class HttpTimelinePost
    {
        public HttpTimelinePost() { }

        public HttpTimelinePost(long id, HttpTimelinePostContent? content, bool deleted, DateTime time, HttpUser? author, DateTime lastUpdated)
        {
            Id = id;
            Content = content;
            Deleted = deleted;
            Time = time;
            Author = author;
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
        /// Last updated time.
        /// </summary>
        public DateTime LastUpdated { get; set; } = default!;
    }

    /// <summary>
    /// Info of a timeline.
    /// </summary>
    public class HttpTimeline
    {
        public HttpTimeline() { }

        public HttpTimeline(string uniqueId, string title, string name, DateTime nameLastModifed, string description, HttpUser owner, TimelineVisibility visibility, List<HttpUser> members, DateTime createTime, DateTime lastModified, bool isHighlight, bool isBookmark, HttpTimelineLinks links)
        {
            UniqueId = uniqueId;
            Title = title;
            Name = name;
            NameLastModifed = nameLastModifed;
            Description = description;
            Owner = owner;
            Visibility = visibility;
            Members = members;
            CreateTime = createTime;
            LastModified = lastModified;
            IsHighlight = isHighlight;
            IsBookmark = isBookmark;
            _links = links;
        }

        /// <summary>
        /// Unique id.
        /// </summary>
        public string UniqueId { get; set; } = default!;
        /// <summary>
        /// Title.
        /// </summary>
        public string Title { get; set; } = default!;
        /// <summary>
        /// Name of timeline.
        /// </summary>
        public string Name { get; set; } = default!;
        /// <summary>
        /// Last modified time of timeline name.
        /// </summary>
        public DateTime NameLastModifed { get; set; } = default!;
        /// <summary>
        /// Timeline description.
        /// </summary>
        public string Description { get; set; } = default!;
        /// <summary>
        /// Owner of the timeline.
        /// </summary>
        public HttpUser Owner { get; set; } = default!;
        /// <summary>
        /// Visibility of the timeline.
        /// </summary>
        public TimelineVisibility Visibility { get; set; }
#pragma warning disable CA2227 // Collection properties should be read only
        /// <summary>
        /// Members of timeline.
        /// </summary>
        public List<HttpUser> Members { get; set; } = default!;
#pragma warning restore CA2227 // Collection properties should be read only
        /// <summary>
        /// Create time of timeline.
        /// </summary>
        public DateTime CreateTime { get; set; } = default!;
        /// <summary>
        /// Last modified time of timeline.
        /// </summary>
        public DateTime LastModified { get; set; } = default!;

        public bool IsHighlight { get; set; }

        public bool IsBookmark { get; set; }

#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>
        /// Related links.
        /// </summary>
        public HttpTimelineLinks _links { get; set; } = default!;
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }

    /// <summary>
    /// Related links for timeline.
    /// </summary>
    public class HttpTimelineLinks
    {
        public HttpTimelineLinks() { }

        public HttpTimelineLinks(string self, string posts)
        {
            Self = self;
            Posts = posts;
        }

        /// <summary>
        /// Self.
        /// </summary>
        public string Self { get; set; } = default!;
        /// <summary>
        /// Posts url.
        /// </summary>
        public string Posts { get; set; } = default!;
    }
}
