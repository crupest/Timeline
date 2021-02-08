using System;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
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
