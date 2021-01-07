namespace Timeline.Models
{
    public enum TimelineVisibility
    {
        /// <summary>
        /// All people including those without accounts.
        /// </summary>
        Public,
        /// <summary>
        /// Only people signed in.
        /// </summary>
        Register,
        /// <summary>
        /// Only member.
        /// </summary>
        Private
    }

    public static class TimelinePostContentTypes
    {
        public const string Text = "text";
        public const string Image = "image";
    }
}
