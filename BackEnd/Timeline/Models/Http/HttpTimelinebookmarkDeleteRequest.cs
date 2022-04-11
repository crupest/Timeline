using System;
using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    public class HttpTimelinebookmarkDeleteRequest
    {
        [Required]
        [Username]
        public string TimelineOwner { get; set; } = default!;

        [Required]
        [TimelineName]
        public string TimelineName { get; set; } = default!;
    }
}

