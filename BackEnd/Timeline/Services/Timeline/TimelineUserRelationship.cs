namespace Timeline.Services.Timeline
{
    public class TimelineUserRelationship
    {
        public TimelineUserRelationship(TimelineUserRelationshipType type, long userId)
        {
            Type = type;
            UserId = userId;
        }

        public TimelineUserRelationshipType Type { get; set; }
        public long UserId { get; set; }
    }
}
