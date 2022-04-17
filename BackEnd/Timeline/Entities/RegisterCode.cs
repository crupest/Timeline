using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("register_code")]
    public class RegisterCode
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("owner_id")]
        public long? OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public UserEntity? Owner { get; set; } = default!;

        [Column("code")]
        public string Code { get; set; } = default!;

        [Column("enabled")]
        public bool Enabled { get; set; }
    }
}
