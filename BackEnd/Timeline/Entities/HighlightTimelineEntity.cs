using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("highlight_timelines")]
    public record HighlightTimelineEntity
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("timeline_id")]
        public long TimelineId { get; set; }

        [ForeignKey(nameof(TimelineId))]
        public TimelineEntity Timeline { get; set; } = default!;

        [Column("operator_id")]
        public long? OperatorId { get; set; }

        [ForeignKey(nameof(OperatorId))]
        public UserEntity? Operator { get; set; }
    }
}
