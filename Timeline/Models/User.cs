namespace Timeline.Models
{
    public class User
    {
        public string? Username { get; set; }
        public string? Nickname { get; set; }

        #region adminsecret
        public bool? Administrator { get; set; }
        #endregion adminsecret

        #region secret
        public long? Id { get; set; }
        public string? Password { get; set; }
        public long? Version { get; set; }
        #endregion secret
    }
}
