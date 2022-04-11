using System;
using System.ComponentModel.DataAnnotations;

namespace Timeline.Models.Http
{
    public class HttpTimelineBookmarkVisibility
    {
        [Required]
        public TimelineVisibility Visibility { get; set; }
    }
}

