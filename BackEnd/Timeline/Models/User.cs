using System;
using Timeline.Services;

namespace Timeline.Models
{
    public record User
    {
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
