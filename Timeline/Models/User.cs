using System;

namespace Timeline.Models
{
    public class User
    {
        public string? UniqueId { get; set; }
        public string? Username { get; set; }
        public string? Nickname { get; set; }
        public bool? Administrator { get; set; }

        #region secret
        public long? Id { get; set; }
        public string? Password { get; set; }
        public long? Version { get; set; }
        public DateTimeOffset? UsernameChangeTime { get; set; }
        public DateTimeOffset? CreateTime { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        #endregion secret
    }
}
