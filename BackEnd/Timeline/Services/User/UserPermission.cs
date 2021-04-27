namespace Timeline.Services.User
{
    public enum UserPermission
    {
        /// <summary>
        /// This permission allows to manage user (creating, deleting or modifying).
        /// </summary>
        UserManagement,
        /// <summary>
        /// This permission allows to view and modify all timelines.
        /// </summary>
        AllTimelineManagement,
        /// <summary>
        /// This permission allow to add or remove highlight timelines.
        /// </summary>
        HighlightTimelineManagement
    }
}
