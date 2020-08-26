using System;
using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Content of post create request.
    /// </summary>
    public class TimelinePostCreateRequestContent
    {
        /// <summary>
        /// Type of post content.
        /// </summary>
        [Required]
        public string Type { get; set; } = default!;
        /// <summary>
        /// If post is of text type, this is the text.
        /// </summary>
        public string? Text { get; set; }
        /// <summary>
        /// If post is of image type, this is base64 of image data.
        /// </summary>
        public string? Data { get; set; }
    }

    public class TimelinePostCreateRequest
    {
        /// <summary>
        /// Content of the new post.
        /// </summary>
        [Required]
        public TimelinePostCreateRequestContent Content { get; set; } = default!;

        /// <summary>
        /// Time of the post. If not set, current time will be used.
        /// </summary>
        public DateTime? Time { get; set; }
    }

    /// <summary>
    /// Create timeline request model.
    /// </summary>
    public class TimelineCreateRequest
    {
        /// <summary>
        /// Name of the new timeline. Must be a valid name.
        /// </summary>
        [Required]
        [TimelineName]
        public string Name { get; set; } = default!;
    }

    /// <summary>
    /// Patch timeline request model.
    /// </summary>
    public class TimelinePatchRequest
    {
        /// <summary>
        /// New title. Null for not change.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// New description. Null for not change.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// New visibility. Null for not change.
        /// </summary>
        public TimelineVisibility? Visibility { get; set; }
    }
}
