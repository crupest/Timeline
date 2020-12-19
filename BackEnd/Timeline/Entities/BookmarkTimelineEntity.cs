using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeline.Entities
{
    [Table("bookmark_timelines")]
    public class BookmarkTimelineEntity
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("timeline")]
        public long TimelineId { get; set; }

        [ForeignKey(nameof(TimelineId))]
        public TimelineEntity Timeline { get; set; } = default!;

        [Column("user")]
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = default!;

        // I don't use order any more since keyword name conflict.
        [Column("rank")]
        public long Rank { get; set; }
    }
}
