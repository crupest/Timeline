using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("timeline_members")]
    public class TimelineMemberEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("user")]
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;

        [Column("timeline")]
        public long TimelineId { get; set; }

        [ForeignKey(nameof(TimelineId))]
        public TimelineEntity Timeline { get; set; } = default!;
    }
}
