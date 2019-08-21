using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("user_details")]
    public class UserDetailEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("qq"), MaxLength(15)]
        public string QQ { get; set; }

        [Column("email"), MaxLength(30)]
        public string EMail { get; set; }

        [Column("phone_number"), MaxLength(15)]
        public string PhoneNumber { get; set; }

        [Column("description")]
        public string Description { get; set; }

        public long UserId { get; set; }
    }
}
