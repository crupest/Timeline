using System;
using System.ComponentModel.DataAnnotations;

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
    }
}
