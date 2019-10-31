using Microsoft.EntityFrameworkCore;

namespace Timeline.Entities
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(e => e.Version).HasDefaultValue(0);
            modelBuilder.Entity<User>().HasIndex(e => e.Name).IsUnique();
        }

        public DbSet<User> Users { get; set; } = default!;
        public DbSet<UserAvatar> UserAvatars { get; set; } = default!;
        public DbSet<UserDetail> UserDetails { get; set; } = default!;
    }
}
