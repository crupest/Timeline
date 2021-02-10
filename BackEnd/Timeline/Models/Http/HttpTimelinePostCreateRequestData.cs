using System.ComponentModel.DataAnnotations;

namespace Timeline.Models.Http
{
    public class HttpTimelinePostCreateRequestData
    {
        /// <summary>
        /// Mime type of the data.
        /// </summary>
        [Required]
        public string ContentType { get; set; } = default!;

        /// <summary>
        /// Base64 of data.
        /// </summary>
        [Required]
        public string Data { get; set; } = default!;
    }
}
