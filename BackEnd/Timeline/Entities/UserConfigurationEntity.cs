using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Timeline.Models;

namespace Timeline.Entities
{
    [Table("user_config")]
    public class UserConfigurationEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = default!;

        [Column("bookmark_visibility")]
        public TimelineVisibility BookmarkVisibility { get; set; }
    }
}
