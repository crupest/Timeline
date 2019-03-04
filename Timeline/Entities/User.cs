namespace Timeline.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string[] Roles { get; set; }

        public UserInfo GetUserInfo()
        {
            return new UserInfo
            {
                Username = Username,
                Roles = Roles
            };
        }
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string[] Roles { get; set; }
    }
}
