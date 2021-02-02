using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Move highlight timeline request body model.
    /// </summary>
    public class HttpHighlightTimelineMoveRequest
    {
        /// <summary>
        /// Timeline name.
        /// </summary>
        [GeneralTimelineName]
        public string Timeline { get; set; } = default!;

        /// <summary>
        /// New position, starting at 1.
        /// </summary>
        [Required]
        public long? NewPosition { get; set; }
    }
}
