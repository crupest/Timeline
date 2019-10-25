using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Timeline.Entities
{
    [Table("user_details")]
    public class UserDetail
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("nickname"), MaxLength(26)]
        public string? Nickname { get; set; }

        public long UserId { get; set; }
    }
}
