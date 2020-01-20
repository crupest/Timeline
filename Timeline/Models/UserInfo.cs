namespace Timeline.Models
{
    public class UserInfo
    {
        public long Id { get; set; }
        public long Version { get; set; }
        public string Username { get; set; } = default!;
        public bool Administrator { get; set; }
    }
}
