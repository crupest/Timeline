using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("user_avatars")]
    public class UserAvatar
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("data"), Required]
        public byte[] Data { get; set; }

        [Column("type"), Required]
        public string Type { get; set; }
    }
}
