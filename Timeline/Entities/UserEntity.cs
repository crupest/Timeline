using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    public static class UserRoles
    {
        public const string Admin = "admin";
        public const string User = "user";
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is an entity class.")]
    [Table("users")]
    public class UserEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("username"), MaxLength(26), Required]
        public string Username { get; set; } = default!;

        [Column("password"), Required]
        public string Password { get; set; } = default!;

        [Column("roles"), Required]
        public string Roles { get; set; } = default!;

        [Column("version"), Required]
        public long Version { get; set; }

        [Column("nickname"), MaxLength(100)]
        public string? Nickname { get; set; }

        public UserAvatarEntity? Avatar { get; set; }

        public List<TimelineEntity> Timelines { get; set; } = default!;

        public List<TimelinePostEntity> TimelinePosts { get; set; } = default!;

        public List<TimelineMemberEntity> TimelinesJoined { get; set; } = default!;
    }
}
