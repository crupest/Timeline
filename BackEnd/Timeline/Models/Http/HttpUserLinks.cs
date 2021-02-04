namespace Timeline.Models.Http
{

    /// <summary>
    /// Related links for user.
    /// </summary>
    public class HttpUserLinks
    {
        public HttpUserLinks() { }

        public HttpUserLinks(string self, string avatar, string timeline)
        {
            Self = self;
            Avatar = avatar;
            Timeline = timeline;
        }

        /// <summary>
        /// Self.
        /// </summary>
        public string Self { get; set; } = default!;
        /// <summary>
        /// Avatar url.
        /// </summary>
        public string Avatar { get; set; } = default!;
        /// <summary>
        /// Personal timeline url.
        /// </summary>
        public string Timeline { get; set; } = default!;
    }
}
