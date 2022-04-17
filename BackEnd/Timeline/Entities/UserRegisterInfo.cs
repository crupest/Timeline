using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("user_register_info")]
    public class UserRegisterInfo
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = default!;

        [Column("register_code")]
        public string RegisterCode { get; set; } = default!;

        [Column("introducer_id")]
        public long? IntroducerId { get; set; }

        [ForeignKey(nameof(IntroducerId))]
        public UserEntity? Introducer { get; set; }

        [Column("register_time")]
        public DateTime RegisterTime { get; set; }
    }
}
