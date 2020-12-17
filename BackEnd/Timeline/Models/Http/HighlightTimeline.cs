using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Move highlight timeline request body model.
    /// </summary>
    public class HttpHighlightTimelineMoveRequest
    {
        [GeneralTimelineName]
        public string Timeline { get; set; } = default!;

        [Required]
        public long? NewPosition { get; set; }
    }
}
