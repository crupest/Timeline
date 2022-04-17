using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Timeline.Services.User;

namespace Timeline.Entities
{
    [Table("users")]
    public class UserEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("unique_id"), Required]
        public string UniqueId { get; set; } = default!;

        [Column("username"), Required]
        public string Username { get; set; } = default!;

        [Column("username_change_time")]
        public DateTime UsernameChangeTime { get; set; }

        [Column("password"), Required]
        public string Password { get; set; } = default!;

        [Column("version"), Required]
        public long Version { get; set; }

        [Column("nickname")]
        public string? Nickname { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        [Column("last_modified")]
        public DateTime LastModified { get; set; }

        public UserAvatarEntity? Avatar { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        /// <summary>
        /// Do not use this directly. Get permissions with <see cref="IUserPermissionService"/>.
        /// </summary>
        [Obsolete("Use IUserPermissionService instead.")]
        public List<UserPermissionEntity> Permissions { get; set; } = default!;

        public List<TimelineEntity> Timelines { get; set; } = default!;

        public List<TimelinePostEntity> TimelinePosts { get; set; } = default!;

        public List<TimelineMemberEntity> TimelinesJoined { get; set; } = default!;

        internal object MapToHttp(IUrlHelper url)
        {
            throw new NotImplementedException();
        }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
