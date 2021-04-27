namespace Timeline.Services.User
{
    /// <summary>
    /// Null means not change.
    /// </summary>
    public class ModifyUserParams
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Nickname { get; set; }
    }
}
