using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Models
{
    [Table("user")]
    public class User
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("name"), Required]
        public string Name { get; set; }

        [Column("password"), Required]
        public string EncryptedPassword { get; set; }

        [Column("roles"), Required]
        public string RoleString { get; set; }
    }

    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
    }
}
