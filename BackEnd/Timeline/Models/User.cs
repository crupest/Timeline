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
        public DateTime? UsernameChangeTime { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? LastModified { get; set; }
        #endregion secret
    }
}
