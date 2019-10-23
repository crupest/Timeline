namespace Timeline.Models
{
    public sealed class UserInfo
    {
        public UserInfo()
        {
        }

        public UserInfo(string username, bool administrator)
        {
            Username = username;
            Administrator = administrator;
        }

        public string Username { get; set; } = default!;
        public bool Administrator { get; set; } = default!;

        public override string ToString()
        {
            return $"Username: {Username} ; Administrator: {Administrator}";
        }
    }
}
