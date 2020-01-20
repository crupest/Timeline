using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("user_details")]
    public class UserDetailEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("nickname"), MaxLength(26)]
        public string? Nickname { get; set; }

        public long UserId { get; set; }
    }
}
