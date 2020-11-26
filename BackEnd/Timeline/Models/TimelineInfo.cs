using System;
using System.Collections.Generic;

namespace Timeline.Models
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

        /// <summary>
        /// The tag of the data. The tag of the entry in DataManager. Also the etag (not quoted).
        /// </summary>
        public string DataTag { get; set; }
    }

    public class TimelinePostInfo
    {
        public TimelinePostInfo()
        {

        }

        public TimelinePostInfo(long id, ITimelinePostContent? content, DateTime time, UserInfo? author, DateTime lastUpdated, string timelineName)
        {
            Id = id;
            Content = content;
            Time = time;
            Author = author;
            LastUpdated = lastUpdated;
            TimelineName = timelineName;
        }

        public long Id { get; set; }
        public ITimelinePostContent? Content { get; set; }
        public bool Deleted => Content == null;
        public DateTime Time { get; set; }
        public UserInfo? Author { get; set; }
        public DateTime LastUpdated { get; set; }
        public string TimelineName { get; set; } = default!;
    }

    public class TimelineInfo
    {
        public TimelineInfo()
        {

        }

        public TimelineInfo(
            string uniqueId,
            string name,
            DateTime nameLastModified,
            string title,
            string description,
            UserInfo owner,
            TimelineVisibility visibility,
            List<UserInfo> members,
            DateTime createTime,
            DateTime lastModified)
        {
            UniqueId = uniqueId;
            Name = name;
            NameLastModified = nameLastModified;
            Title = title;
            Description = description;
            Owner = owner;
            Visibility = visibility;
            Members = members;
            CreateTime = createTime;
            LastModified = lastModified;
        }

        public string UniqueId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public DateTime NameLastModified { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public UserInfo Owner { get; set; } = default!;
        public TimelineVisibility Visibility { get; set; }
#pragma warning disable CA2227 // Collection properties should be read only
        public List<UserInfo> Members { get; set; } = default!;
#pragma warning restore CA2227 // Collection properties should be read only
        public DateTime CreateTime { get; set; } = default!;
        public DateTime LastModified { get; set; } = default!;
    }

    public class TimelineChangePropertyRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TimelineVisibility? Visibility { get; set; }
    }
}
