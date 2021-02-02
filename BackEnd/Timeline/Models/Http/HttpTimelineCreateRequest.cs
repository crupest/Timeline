using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Create timeline request model.
    /// </summary>
    public class HttpTimelineCreateRequest
    {
        /// <summary>
        /// Name of the new timeline. Must be a valid name.
        /// </summary>
        [Required]
        [TimelineName]
        public string Name { get; set; } = default!;
    }
}
