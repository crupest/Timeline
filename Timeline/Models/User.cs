using Timeline.Models.Validation;

namespace Timeline.Models
{
    public class User
    {
        [Username]
        public string? Username { get; set; }
        public bool? Administrator { get; set; }
        public string? Nickname { get; set; }
        public string? AvatarUrl { get; set; }


        #region secret
        public string? Password { get; set; }
        public long? Version { get; set; }
        #endregion secret
    }
}
