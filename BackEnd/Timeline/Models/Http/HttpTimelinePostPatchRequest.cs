using System;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    /// <summary>
    /// The model of changing post content.
    /// </summary>
    public class HttpTimelinePostPatchRequestContent
    {
        /// <summary>
        /// The new type of the post. If null, old type is used. This field can't be used alone. Use it with corresponding fields to change post content.
        /// </summary>
        [TimelinePostContentType]
        public string? Type { get; set; }
        /// <summary>
        /// The new text. Null for not change.
        /// </summary>
        public string? Text { get; set; }
        /// <summary>
        /// The new data. Null for not change.
        /// </summary>
        public string? Data { get; set; }
    }

    public class HttpTimelinePostPatchRequest
    {
        /// <summary>
        /// Change the time. Null for not change.
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// Change the color. Null for not change.
        /// </summary>
        [Color]
        public string? Color { get; set; }
    }
}
