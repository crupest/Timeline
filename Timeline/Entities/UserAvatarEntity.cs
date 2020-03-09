using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimelineApp.Entities
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This is data base entity.")]
    [Table("user_avatars")]
    public class UserAvatarEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("data_tag")]
        public string? DataTag { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        [Column("last_modified"), Required]
        public DateTime LastModified { get; set; }

        [Column("user"), Required]
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = default!;
    }
}
