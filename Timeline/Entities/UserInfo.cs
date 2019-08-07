namespace Timeline.Entities
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

        public string Username { get; set; }
        public bool Administrator { get; set; }

        public override string ToString()
        {
            return $"Username: {Username} ; Administrator: {Administrator}";
        }
    }
}
