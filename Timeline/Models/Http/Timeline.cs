using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Timeline.Models.Http
{
    public class TimelinePostCreateRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Content { get; set; } = default!;

        public DateTime? Time { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is a DTO class.")]
    public class TimelineMemberChangeRequest
    {
        public List<string>? Add { get; set; }

        public List<string>? Remove { get; set; }
    }
}
