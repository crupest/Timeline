using System;
using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    public class TimelinePostCreateRequest
    {
        [Required(AllowEmptyStrings = true)]
        public string Content { get; set; } = default!;

        public DateTime? Time { get; set; }
    }

    public class TimelineCreateRequest
    {
        [Required]
        [TimelineName]
        public string Name { get; set; } = default!;
    }

    public class TimelinePatchRequest
    {
        public string? Description { get; set; }

        public TimelineVisibility? Visibility { get; set; }
    }
}
