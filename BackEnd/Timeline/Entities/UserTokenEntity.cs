using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("user_token")]
    public class UserTokenEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = default!;

        [Column("token")]
        public string Token { get; set; } = default!;

        [Column("expire_at")]
        public DateTime? ExpireAt { get; set; }

        [Column("create_at")]
        public DateTime? CreateAt { get; set; }
    }
}
