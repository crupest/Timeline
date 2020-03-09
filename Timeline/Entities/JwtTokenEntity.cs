using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimelineApp.Entities
{
    [Table("jwt_token")]
    public class JwtTokenEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required, Column("key")]
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Key { get; set; } = default!;
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
