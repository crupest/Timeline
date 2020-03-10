using Microsoft.EntityFrameworkCore;

namespace Timeline.Entities
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>().Property(e => e.Version).HasDefaultValue(0);
            modelBuilder.Entity<UserEntity>().HasIndex(e => e.Username).IsUnique();
            modelBuilder.Entity<DataEntity>().HasIndex(e => e.Tag).IsUnique();
        }

        public DbSet<UserEntity> Users { get; set; } = default!;
        public DbSet<UserAvatarEntity> UserAvatars { get; set; } = default!;
        public DbSet<TimelineEntity> Timelines { get; set; } = default!;
        public DbSet<TimelinePostEntity> TimelinePosts { get; set; } = default!;
        public DbSet<TimelineMemberEntity> TimelineMembers { get; set; } = default!;
        public DbSet<JwtTokenEntity> JwtToken { get; set; } = default!;
        public DbSet<DataEntity> Data { get; set; } = default!;
    }
}
