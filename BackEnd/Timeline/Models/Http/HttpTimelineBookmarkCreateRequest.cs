using System;
using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    public class HttpTimelineBookmarkCreateRequest
    {
        [Required]
        [Username]
        public string TimelineOwner { get; set; } = default!;

        [Required]
        [TimelineName]
        public string TimelineName { get; set; } = default!;

        public int? Position { get; set; }
    }
}

