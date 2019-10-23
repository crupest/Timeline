using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    public static class UserRoles
    {
        public const string Admin = "admin";
        public const string User = "user";
    }

    [Table("users")]
    public class User
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("name"), MaxLength(26), Required]
        public string Name { get; set; } = default!;

        [Column("password"), Required]
        public string EncryptedPassword { get; set; } = default!;

        [Column("roles"), Required]
        public string RoleString { get; set; } = default!;

        [Column("version"), Required]
        public long Version { get; set; }

        public UserAvatar? Avatar { get; set; }
    }
}
