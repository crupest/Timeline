using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Models
{
    public class TimelinePostInfo
    {
        public long Id { get; set; }

        public string? Content { get; set; }

        public DateTime Time { get; set; }

        /// <summary>
        /// The username of the author.
        /// </summary>
        public string Author { get; set; } = default!;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is a DTO class.")]
    public class BaseTimelineInfo
    {
        public string? Description { get; set; }

        /// <summary>
        /// The username of the owner.
        /// </summary>
        public string Owner { get; set; } = default!;

        public TimelineVisibility Visibility { get; set; }

        public List<string> Members { get; set; } = default!;
    }

    public class TimelineInfo : BaseTimelineInfo
    {
        public string Name { get; set; } = default!;
    }
}
