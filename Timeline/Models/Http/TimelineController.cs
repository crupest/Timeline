using System;
using System.ComponentModel.DataAnnotations;
using TimelineApp.Models.Validation;

namespace TimelineApp.Models.Http
{
    public class TimelinePostCreateRequestContent
    {
        [Required]
        public string Type { get; set; } = default!;
        public string? Text { get; set; }
        public string? Data { get; set; }
    }

    public class TimelinePostCreateRequest
    {
        public TimelinePostCreateRequestContent Content { get; set; } = default!;

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
