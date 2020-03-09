using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimelineApp.Entities
{
    [Table("data")]
    public class DataEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("tag"), Required]
        public string Tag { get; set; } = default!;

        [Column("data"), Required]
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; set; } = default!;
#pragma warning restore CA1819 // Properties should not return arrays

        [Column("ref"), Required]
        public int Ref { get; set; }
    }
}
