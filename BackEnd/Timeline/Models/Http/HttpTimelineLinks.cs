namespace Timeline.Models.Http
{

    /// <summary>
    /// Related links for timeline.
    /// </summary>
    public class HttpTimelineLinks
    {
        public HttpTimelineLinks() { }

        public HttpTimelineLinks(string self, string posts)
        {
            Self = self;
            Posts = posts;
        }

        /// <summary>
        /// Self.
        /// </summary>
        public string Self { get; set; } = default!;
        /// <summary>
        /// Posts url.
        /// </summary>
        public string Posts { get; set; } = default!;
    }
}
