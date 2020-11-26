using System;
using Timeline.Services;

namespace Timeline.Models
{
    public class UserInfo
    {
        public UserInfo()
        {

        }

        public UserInfo(
            long id,
            string uniqueId,
            string username,
            string nickname,
            UserPermissions permissions,
            DateTime usernameChangeTime,
            DateTime createTime,
            DateTime lastModified,
            long version)
        {
            Id = id;
            UniqueId = uniqueId;
            Username = username;
            Nickname = nickname;
            Permissions = permissions;
            UsernameChangeTime = usernameChangeTime;
            CreateTime = createTime;
            LastModified = lastModified;
            Version = version;
        }

        public long Id { get; set; }
        public string UniqueId { get; set; } = default!;

        public string Username { get; set; } = default!;
        public string Nickname { get; set; } = default!;

        public UserPermissions Permissions { get; set; } = default!;

        public DateTime UsernameChangeTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastModified { get; set; }
        public long Version { get; set; }
    }
}
