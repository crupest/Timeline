using Microsoft.EntityFrameworkCore;
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
        public string Name { get; set; }

        [Column("password"), Required]
        public string EncryptedPassword { get; set; }

        [Column("roles"), Required]
        public string RoleString { get; set; }

        [Column("version"), Required]
        public long Version { get; set; }

        [Required]
        public UserAvatar Avatar { get; set; }
    }

    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(e => e.Version).HasDefaultValue(0);
            modelBuilder.Entity<User>().HasIndex(e => e.Name).IsUnique();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserAvatar> UserAvatars { get; set; }
    }
}
