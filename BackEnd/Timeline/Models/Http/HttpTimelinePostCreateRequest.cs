using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    public class HttpTimelinePostCreateRequestData
    {
        /// <summary>
        /// Kind of the data.
        /// </summary>
        [Required]
        [TimelinePostDataKind]
        public string Kind { get; set; } = default!;

        /// <summary>
        /// The true data. If kind is text or markdown, this is a string. If kind is image, this is base64 of data.
        /// </summary>
        [Required]
        public string Data { get; set; } = default!;
    }

    public class HttpTimelinePostCreateRequest
    {
        /// <summary>
        /// Data list of the new content.
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<HttpTimelinePostCreateRequestData> DataList { get; set; } = default!;

        /// <summary>
        /// Time of the post. If not set, current time will be used.
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// Color of the post.
        /// </summary>
        [Color]
        public string? Color { get; set; }
    }
}
