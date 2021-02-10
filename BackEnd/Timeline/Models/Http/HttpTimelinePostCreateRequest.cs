using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    public class HttpTimelinePostCreateRequest
    {
        /// <summary>
        /// Data list of the new content.
        /// </summary>
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
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
