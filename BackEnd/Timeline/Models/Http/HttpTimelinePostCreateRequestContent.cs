using System.ComponentModel.DataAnnotations;
using Timeline.Models.Validation;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Content of post create request.
    /// </summary>
    public class HttpTimelinePostCreateRequestContent
    {
        /// <summary>
        /// Type of post content.
        /// </summary>
        [Required]
        [TimelinePostContentType]
        public string Type { get; set; } = default!;
        /// <summary>
        /// If post is of text type, this is the text.
        /// </summary>
        public string? Text { get; set; }
        /// <summary>
        /// If post is of image type, this is base64 of image data.
        /// </summary>
        public string? Data { get; set; }
    }
}
