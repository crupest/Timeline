using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This is data base entity.")]
    [Table("user_avatars")]
    public class UserAvatar
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("data")]
        public byte[]? Data { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        [Column("etag"), MaxLength(30)]
        public string? ETag { get; set; }

        [Column("last_modified"), Required]
        public DateTime LastModified { get; set; }

        public long UserId { get; set; }
    }
}
