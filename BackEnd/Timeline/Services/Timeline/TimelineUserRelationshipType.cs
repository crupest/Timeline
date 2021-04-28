namespace Timeline.Services.Timeline
{
    public enum TimelineUserRelationshipType
    {
        Own = 0b1,
        Join = 0b10,
        Default = Own | Join
    }
}
