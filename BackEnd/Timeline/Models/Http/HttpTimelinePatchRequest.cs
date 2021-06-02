using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Patch timeline request model.
    /// </summary>
    public class HttpTimelinePatchRequest
    {
        /// <summary>
        /// New name. Null for not change.
        /// </summary>
        [TimelineName]
        public string? Name { get; set; }

        /// <summary>
        /// New title. Null for not change.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// New description. Null for not change.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// New visibility. Null for not change.
        /// </summary>
        public TimelineVisibility? Visibility { get; set; }

        /// <summary>
        /// New color. Null for not change.
        /// </summary>
        [Color(PermitDefault = true, PermitEmpty = true)]
        public string? Color { get; set; }
    }
}
