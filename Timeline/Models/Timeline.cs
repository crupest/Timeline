﻿using System;
using System.Collections.Generic;

namespace TimelineApp.Models
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
        public TextTimelinePostContent(string text) { Text = text; }

        public string Type { get; } = TimelinePostContentTypes.Text;
        public string Text { get; set; }
    }

    public class ImageTimelinePostContent : ITimelinePostContent
    {
        public ImageTimelinePostContent(string dataTag) { DataTag = dataTag; }

        public string Type { get; } = TimelinePostContentTypes.Image;
        public string DataTag { get; set; }
    }

    public class TimelinePost
    {
        public long Id { get; set; }
        public ITimelinePostContent Content { get; set; } = default!;
        public DateTime Time { get; set; }
        public User Author { get; set; } = default!;
        public DateTime LastUpdated { get; set; } = default!;
    }

    public class Timeline
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public User Owner { get; set; } = default!;
        public TimelineVisibility Visibility { get; set; }
#pragma warning disable CA2227 // Collection properties should be read only
        public List<User> Members { get; set; } = default!;
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class TimelineChangePropertyRequest
    {
        public string? Description { get; set; }
        public TimelineVisibility? Visibility { get; set; }
    }
}
