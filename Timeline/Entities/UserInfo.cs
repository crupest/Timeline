namespace Timeline.Entities
{
    public sealed class UserInfo
    {
        public UserInfo()
        {
        }

        public UserInfo(string username, bool isAdmin)
        {
            Username = username;
            IsAdmin = isAdmin;
        }

        public string Username { get; set; }
        public bool IsAdmin { get; set; }

        public override string ToString()
        {
            return $"Username: {Username} ; IsAdmin: {IsAdmin}";
        }
    }
}
