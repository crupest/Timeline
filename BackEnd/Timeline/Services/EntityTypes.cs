namespace Timeline.Services
{
    public static class EntityTypes
    {
        public static EntityType Default { get; } = new EntityType(EntityNames.Default);
        public static EntityType User { get; } = new EntityType(EntityNames.User);
        public static EntityType Timeline { get; } = new EntityType(EntityNames.Timeline);
        public static EntityType TimelinePost { get; } = new EntityType(EntityNames.TimelinePost);
        public static EntityType TimelinePostData { get; } = new EntityType(EntityNames.TimelinePostData);
        public static EntityType BookmarkTimeline { get; } = new EntityType(EntityNames.BookmarkTimeline);
        public static EntityType HighlightTimeline { get; } = new EntityType(EntityNames.HighlightTimeline);
    }
}
