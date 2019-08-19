﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("user_avatars")]
    public class UserAvatar
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("data")]
        public byte[] Data { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("last_modified"), Required]
        public DateTime LastModified { get; set; }

        public long UserId { get; set; }

        public static UserAvatar Create(DateTime lastModified)
        {
            return new UserAvatar
            {
                Id = 0,
                Data = null,
                Type = null,
                LastModified = lastModified
            };
        }
    }
}
