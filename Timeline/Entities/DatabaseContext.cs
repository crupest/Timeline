using Microsoft.EntityFrameworkCore;

namespace Timeline.Entities
{
    public abstract class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>().Property(e => e.Version).HasDefaultValue(0);
            modelBuilder.Entity<UserEntity>().HasIndex(e => e.Username).IsUnique();
        }

        public DbSet<UserEntity> Users { get; set; } = default!;
        public DbSet<UserAvatarEntity> UserAvatars { get; set; } = default!;
        public DbSet<TimelineEntity> Timelines { get; set; } = default!;
        public DbSet<TimelinePostEntity> TimelinePosts { get; set; } = default!;
        public DbSet<TimelineMemberEntity> TimelineMembers { get; set; } = default!;
    }
    public class ProductionDatabaseContext : DatabaseContext
    {
        public ProductionDatabaseContext(DbContextOptions<ProductionDatabaseContext> options)
            : base(options)
        {

        }
    }

    public class DevelopmentDatabaseContext : DatabaseContext
    {
        public DevelopmentDatabaseContext(DbContextOptions<DevelopmentDatabaseContext> options)
            : base(options)
        {

        }
    }
}
