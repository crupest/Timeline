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
            modelBuilder.Entity<UserEntity>().Property(e => e.UniqueId).HasDefaultValueSql("lower(hex(randomblob(16)))");
            modelBuilder.Entity<UserEntity>().Property(e => e.UsernameChangeTime).HasDefaultValueSql("datetime('now', 'utc')");
            modelBuilder.Entity<UserEntity>().Property(e => e.CreateTime).HasDefaultValueSql("datetime('now', 'utc')");
            modelBuilder.Entity<UserEntity>().Property(e => e.LastModified).HasDefaultValueSql("datetime('now', 'utc')");
            modelBuilder.Entity<DataEntity>().HasIndex(e => e.Tag).IsUnique();
            modelBuilder.Entity<TimelineEntity>().Property(e => e.UniqueId).HasDefaultValueSql("lower(hex(randomblob(16)))");

            modelBuilder.ApplyUtcDateTimeConverter();
        }

        public DbSet<UserEntity> Users { get; set; } = default!;
        public DbSet<UserAvatarEntity> UserAvatars { get; set; } = default!;
        public DbSet<UserPermissionEntity> UserPermission { get; set; } = default!;
        public DbSet<TimelineEntity> Timelines { get; set; } = default!;
        public DbSet<TimelinePostEntity> TimelinePosts { get; set; } = default!;
        public DbSet<TimelineMemberEntity> TimelineMembers { get; set; } = default!;
        public DbSet<HighlightTimelineEntity> HighlightTimelines { get; set; } = default!;
        public DbSet<BookmarkTimelineEntity> BookmarkTimelines { get; set; } = default!;

        public DbSet<JwtTokenEntity> JwtToken { get; set; } = default!;
        public DbSet<DataEntity> Data { get; set; } = default!;
    }
}
