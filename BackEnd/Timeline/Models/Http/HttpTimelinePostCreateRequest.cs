using System;
using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    public class HttpTimelinePostCreateRequest
    {
        /// <summary>
        /// Content of the new post.
        /// </summary>
        [Required]
        public HttpTimelinePostCreateRequestContent Content { get; set; } = default!;

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
