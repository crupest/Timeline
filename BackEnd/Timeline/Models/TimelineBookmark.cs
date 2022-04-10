namespace Timeline.Models
{
    public class TimelineBookmark
    {
        public TimelineBookmark(string timelineOwner, string timelineName, int position)
        {
            TimelineOwner = timelineOwner;
            TimelineName = timelineName;
            Position = position;
        }

        public string TimelineOwner { get; set; }
        public string TimelineName { get; set; }
        public int Position { get; set; }
    }
}
