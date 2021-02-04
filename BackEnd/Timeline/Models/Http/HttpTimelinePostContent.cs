namespace Timeline.Models.Http
{
    /// <summary>
    /// Info of post content.
    /// </summary>
    public class HttpTimelinePostContent
    {
        public HttpTimelinePostContent() { }

        public HttpTimelinePostContent(string type, string? text, string? url, string? eTag)
        {
            Type = type;
            Text = text;
            Url = url;
            ETag = eTag;
        }

        /// <summary>
        /// Type of the post content.
        /// </summary>
        public string Type { get; set; } = default!;
        /// <summary>
        /// If post is of text type. This is the text.
        /// </summary>
        public string? Text { get; set; }
        /// <summary>
        /// If post is of image type. This is the image url.
        /// </summary>
        public string? Url { get; set; }
        /// <summary>
        /// If post has data (currently it means it's a image post), this is the data etag.
        /// </summary>
        public string? ETag { get; set; }
    }
}
