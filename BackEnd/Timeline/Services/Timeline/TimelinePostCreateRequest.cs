using System;
using System.Collections.Generic;

namespace Timeline.Services.Timeline
{
    public class TimelinePostCreateRequest
    {
        public string? Color { get; set; }

        /// <summary>If not set, current time is used.</summary>
        public DateTime? Time { get; set; }

#pragma warning disable CA2227
        public List<TimelinePostCreateRequestData> DataList { get; set; } = new List<TimelinePostCreateRequestData>();
#pragma warning restore CA2227
    }
}
