﻿using System;
using System.Collections.Generic;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Info of a timeline.
    /// </summary>
    public class HttpTimeline
    {
        public HttpTimeline() { }

        public HttpTimeline(string uniqueId, string title, string name, string nameV2, DateTime nameLastModifed, string description, HttpUser owner, TimelineVisibility visibility, List<HttpUser> members, string? color, DateTime createTime, DateTime lastModified, bool isHighlight, bool isBookmark, bool manageable, bool postable, HttpTimelineLinks links)
        {
            UniqueId = uniqueId;
            Title = title;
#pragma warning disable CS0618 // Type or member is obsolete
            Name = name;
#pragma warning restore CS0618 // Type or member is obsolete
            NameV2 = nameV2;
            NameLastModifed = nameLastModifed;
            Description = description;
            Owner = owner;
            Visibility = visibility;
            Members = members;
            Color = color;
            CreateTime = createTime;
            LastModified = lastModified;
            IsHighlight = isHighlight;
            IsBookmark = isBookmark;
            Manageable = manageable;
            Postable = postable;
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
        [Obsolete("Use NameV2")]
        public string Name { get; set; } = default!;
        /// <summary>
        /// Name of timeline.
        /// </summary>
        public string NameV2 { get; set; } = default!;
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
        /// Color of timeline.
        /// </summary>
        public string? Color { get; set; }
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
        public bool Manageable { get; set; }
        public bool Postable { get; set; }

#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>
        /// Related links.
        /// </summary>
        public HttpTimelineLinks _links { get; set; } = default!;
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
