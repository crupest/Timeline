namespace Timeline.Models.Http
{

    /// <summary>
    /// Related links for user.
    /// </summary>
    public class HttpUserLinks
    {
        public HttpUserLinks() { }

        public HttpUserLinks(string self, string avatar)
        {
            Self = self;
            Avatar = avatar;
        }

        /// <summary>
        /// Self.
        /// </summary>
        public string Self { get; set; } = default!;
        /// <summary>
        /// Avatar url.
        /// </summary>
        public string Avatar { get; set; } = default!;
    }
}
