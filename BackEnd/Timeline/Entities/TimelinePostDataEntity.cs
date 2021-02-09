using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("timeline_post_data")]
    public class TimelinePostDataEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("post")]
        public long PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public TimelinePostEntity Timeline { get; set; } = default!;

        [Column("index")]
        public long Index { get; set; }

        [Column("kind")]
        public string Kind { get; set; } = default!;

        [Column("data_tag")]
        public string DataTag { get; set; } = default!;

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
}
